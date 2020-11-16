using Markdig;
using Microsoft.Extensions.Logging;
using Polygon.Entities;
using Polygon.Storages;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Polygon.Packaging
{
    public class KattisImportProvider : ImportProviderBase
    {
        #region Static Visitors

        delegate void Visitor(string token, ImportContext node);

        private static readonly Dictionary<string, Visitor> iniParser;
        private static readonly Dictionary<string, Visitor> yamlParser;

        const int LINUX755 = -2115174400;
        const int LINUX644 = -2119958528;

        static KattisImportProvider()
        {
            iniParser = new Dictionary<string, Visitor>();
            yamlParser = new Dictionary<string, Visitor>();

            iniParser.Add("name", (token, node) =>
            {
                if (!string.IsNullOrEmpty(token))
                    node.Problem.Title = token;
            });

            iniParser.Add("timelimit", (token, node) =>
            {
                if (double.TryParse(token.Trim('"', '\''), out var time))
                {
                    int time2 = (int)Math.Round(time * 1000);
                    if (time2 > 15000 || time2 < 500)
                        node.Log($"Error timelimit: '{time2}' out of range.");
                    else
                        node.Problem.TimeLimit = time2;
                }
                else
                {
                    node.Log($"Error timelimit: parsing '{token}'.");
                }
            });

            yamlParser.Add("name", iniParser["name"]);

            yamlParser.Add("source", (token, node) =>
            {
                if (!string.IsNullOrEmpty(token))
                    node.Problem.Source = token;
            });

            yamlParser.Add("validator_flags", (token, node) =>
            {
                if (!string.IsNullOrEmpty(token))
                    node.Problem.ComapreArguments = token;
            });

            yamlParser.Add("validation", (token, node) =>
            {
                if (token == "custom")
                    node.Flag = 1;
                else if (token == "custom interactive")
                    node.Flag = 2;
            });

            yamlParser.Add("memory", (token, node) =>
            {
                if (int.TryParse(token, out int mem))
                {
                    if (mem > 1024)
                    {
                        mem = 1024;
                        node.Log("memory limit has been cut to 1GB.");
                    }

                    if (mem < 32)
                    {
                        mem = 32;
                        node.Log("memory limit has been enlarged to 32MB.");
                    }

                    node.Problem.MemoryLimit = mem << 10;
                }
            });

            yamlParser.Add("output", (token, node) =>
            {
                if (int.TryParse(token, out int output_limit))
                {
                    if (output_limit > 40)
                    {
                        output_limit = 40;
                        node.Log("output limit has been cut to 40MB.");
                    }

                    if (output_limit < 4)
                    {
                        output_limit = 4;
                        node.Log("output limit has been enlarged to 4MB.");
                    }

                    node.Problem.OutputLimit = output_limit << 10;
                }
            });
        }

        private static async Task ReadLinesAsync(ZipArchiveEntry entry, ImportContext context,
            Dictionary<string, Visitor> parser, char comment, char equal)
        {
            if (entry == null) return;

            using var stream = entry.Open();
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (line == null) break;
                int cmt = line.IndexOf(comment);
                if (cmt != -1) line = line.Substring(0, cmt);
                cmt = line.IndexOf(equal);
                if (cmt == -1) continue;
                var startToken = line.Substring(0, cmt).Trim();
                if (parser.TryGetValue(startToken, out var visitor))
                    visitor.Invoke(line[(cmt + 1)..].Trim(), context);
            }
        }

        private static Dictionary<string, Verdict> Verd =
            new Dictionary<string, Verdict>
            {
                ["accepted"] = Verdict.Accepted,
                ["wrong_answer"] = Verdict.WrongAnswer,
                ["time_limit_exceeded"] = Verdict.TimeLimitExceeded,
                ["run_time_error"] = Verdict.RuntimeError,
            };

        #endregion

        private IMarkdownService Markdown { get; }

        public KattisImportProvider(IPolygonFacade facade, ILogger<KattisImportProvider> logger, IMarkdownService markdown) : base(facade, logger)
        {
            Markdown = markdown;
        }

        private async Task<Executable?> GetOutputValidatorAsync(ImportContext ctx, ZipArchive zip)
        {
            var list = zip.Entries
                .Where(z => z.FullName.StartsWith("output_validators/") && !z.FullName.EndsWith('/'))
                .ToList();

            if (list.Count == 0)
            {
                Log("No output validator found.");
                return null;
            }

            var fileNames = list.FirstOrDefault().FullName.Split(new[] { '/' }, 3);
            if (fileNames.Length != 3)
            {
                Log($"Wrong file found: '{list[0].FullName}', ignoring output validator.");
                return null;
            }

            var prefix = fileNames[0] + '/' + fileNames[1] + '/';
            if (list.Any(z => !z.FullName.StartsWith(prefix)))
            {
                Log($"More than 1 output validator are found, ignoring.");
                return null;
            }

            var stream = new MemoryStream();
            using (var newzip = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                foreach (var file in list)
                {
                    using var fs = file.Open();
                    var fileName = file.FullName[prefix.Length..];
                    var f = await newzip.CreateEntryFromStream(fs, fileName);
                    if (fileName == "build" || fileName == "run")
                        f.ExternalAttributes = LINUX755;
                    else
                        f.ExternalAttributes = file.ExternalAttributes;
                }
            }
            
            stream.Position = 0;
            var content = new byte[stream.Length];
            int pos = 0;
            while (pos < stream.Length)
                pos += await stream.ReadAsync(content, pos, (int)stream.Length - pos);

            return await ctx.AddAsync(new Executable
            {
                Description = $"output validator for p{ctx.Id}",
                ZipFile = content,
                Md5sum = content.ToMD5().ToHexDigest(true),
                ZipSize = pos,
                Id = $"p{ctx.Id}{(ctx.Flag == 1 ? "cmp" : "run")}",
                Type = ctx.Flag == 1 ? "compare" : "run",
            });
        }

        private async Task LoadSubmissionsAsync(ImportContext ctx, ZipArchive zip)
        {
            var prefix = "submissions/";
            var files = zip.Entries
                .Where(z => z.FullName.StartsWith(prefix) && !z.FullName.EndsWith('/'))
                .ToList();
            var langs = await Facade.Languages.ListAsync();

            foreach (var file in files)
            {
                if (file.Length > 65536)
                {
                    Log($"Too big for jury solution '{file.FullName}'");
                    continue;
                }

                var lang = langs.FirstOrDefault(l =>
                    "." + l.FileExtension == Path.GetExtension(file.FullName));

                if (lang == null)
                {
                    Log($"No language found for jury solution '{file.FullName}' Fallback to default.");
                    lang = langs.First();
                }

                var expected = Verd.GetValueOrDefault(file.FullName.Split('/')[1]);
                var code = await file.ReadAsStringAsync();
                var sub = await ctx.SubmitAsync(code, lang.Id, expected);
                Log($"Jury solution '{file.FullName}' saved s{sub.Id}.");
            }
        }

        public override async Task<List<Problem>> ImportAsync(Stream stream, string uploadFileName, string username)
        {
            using var zipArchive = new ZipArchive(stream);

            var ctx = await CreateAsync(new Problem
            {
                AllowJudge = false,
                AllowSubmit = false,
                Title = TryGetPackageName(uploadFileName),
                CompareScript = "compare",
                RunScript = "run",
                MemoryLimit = 524288,
                OutputLimit = 4096,
                Source = username,
                TimeLimit = 10000,
            });

            await ReadLinesAsync(
                entry: zipArchive.GetEntry("domjudge-problem.ini"),
                context: ctx,
                parser: iniParser,
                comment: ';',
                equal: '=');

            await ReadLinesAsync(
                entry: zipArchive.GetEntry("problem.yaml"),
                context: ctx,
                parser: yamlParser,
                comment: '#',
                equal: ':');

            if (ctx.Flag != 0)
            {
                var exec = await GetOutputValidatorAsync(ctx, zipArchive);
                if (exec != null)
                {
                    if (ctx.Flag == 1)
                    {
                        ctx.Problem.CompareScript = exec.Id;
                    }
                    else
                    {
                        ctx.Problem.RunScript = exec.Id;
                        ctx.Problem.CombinedRunCompare = true;
                    }
                }
            }

            // Load statements
            foreach (var mdfile in StorageExtensions.MarkdownFiles)
            {
                var entry = zipArchive.GetEntry("problem_statement/" + mdfile + ".md");
                if (entry == null) continue;

                string mdcontent = await entry.ReadAsStringAsync();

                var tags = $"p{ctx.Id}";
                var content = await (Markdown, StaticFiles).ImportWithImagesAsync(mdcontent, tags);
                await ctx.WriteAsync($"{mdfile}.md", content);

                Log($"Adding statement section 'problem_statement/{mdfile}.md'.");
            }

            // Load testcases
            await ctx.KattisAsync(zipArchive, "data/sample/", false);
            await ctx.KattisAsync(zipArchive, "data/secret/", true);
            Log("All testcases has been added.");

            // Load submissions
            await LoadSubmissionsAsync(ctx, zipArchive);
            Log("All jury solutions has been added.");

            var problem = await ctx.FinalizeAsync();
            return new List<Problem> { problem };
        }
    }
}
