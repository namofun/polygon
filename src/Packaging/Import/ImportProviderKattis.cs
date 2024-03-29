﻿using Markdig;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Storages;

namespace Xylab.Polygon.Packaging
{
    public sealed class KattisImportProvider : ImportProviderBase
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
                    node.Problem.CompareArguments = token;
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

        private static async Task ReadLinesAsync(
            ZipArchiveEntry entry,
            ImportContext context,
            Dictionary<string, Visitor> parser,
            char comment,
            char equal)
        {
            if (entry == null) return;

            using Stream stream = entry.Open();
            using StreamReader reader = new(stream);
            while (!reader.EndOfStream)
            {
                string line = await reader.ReadLineAsync();
                if (line == null) break;
                int cmt = line.IndexOf(comment);
                if (cmt != -1) line = line[..cmt];
                cmt = line.IndexOf(equal);
                if (cmt == -1) continue;
                string startToken = line[..cmt].Trim();
                if (parser.TryGetValue(startToken, out Visitor visitor))
                {
                    visitor.Invoke(line[(cmt + 1)..].Trim(), context);
                }
            }
        }

        private static readonly IReadOnlyDictionary<string, Verdict> Verd =
            new Dictionary<string, Verdict>
            {
                ["accepted"] = Verdict.Accepted,
                ["wrong_answer"] = Verdict.WrongAnswer,
                ["time_limit_exceeded"] = Verdict.TimeLimitExceeded,
                ["run_time_error"] = Verdict.RuntimeError,
            };

        private static readonly IReadOnlyList<string> TexStmt =
            new[]
            {
                "problem.en.tex",
                "problem.zh.tex"
            };

        #endregion

        private IMarkdownService Markdown { get; }
        private IWwwrootFileProvider Files { get; }

        public KattisImportProvider(
            IPolygonFacade facade,
            ILogger<KattisImportProvider> logger,
            IMarkdownService markdown,
            IWwwrootFileProvider files)
            : base(facade, logger)
        {
            Markdown = markdown;
            Files = files;
        }

        private async Task<Executable> GetOutputValidatorAsync(ImportContext ctx, ZipArchive zip)
        {
            List<ZipArchiveEntry> list = zip.Entries
                .Where(z => z.FullName.StartsWith("output_validators/") && !z.FullName.EndsWith('/'))
                .ToList();

            if (list.Count == 0)
            {
                Log("No output validator found.");
                return null;
            }

            string[] fileNames = list.FirstOrDefault().FullName.Split(new[] { '/' }, 3);
            if (fileNames.Length != 3)
            {
                Log($"Wrong file found: '{list[0].FullName}', ignoring output validator.");
                return null;
            }

            string prefix = fileNames[0] + '/' + fileNames[1] + '/';
            if (list.Any(z => !z.FullName.StartsWith(prefix)))
            {
                Log($"More than 1 output validator are found, ignoring.");
                return null;
            }

            MemoryStream stream = new();
            using (ZipArchive newzip = new(stream, ZipArchiveMode.Create, true))
            {
                foreach (ZipArchiveEntry file in list)
                {
                    using Stream fs = file.Open();
                    string fileName = file.FullName[prefix.Length..];
                    ZipArchiveEntry f = await newzip.CreateEntryFromStream(fs, fileName);
                    f.ExternalAttributes = fileName == "build" || fileName == "run" ? LINUX755 : LINUX644;
                }
            }

            stream.Position = 0;
            byte[] content = new byte[stream.Length];
            Memory<byte> memory = new(content);
            for (int pos = 0; pos < stream.Length;)
            {
                pos += await stream.ReadAsync(memory[pos..]);
            }

            return await ctx.AddAsync(new Executable
            {
                Description = $"output validator for p{ctx.Id}",
                ZipFile = content,
                Md5sum = content.ToMD5().ToHexDigest(true),
                ZipSize = (int)stream.Length,
                Id = $"p{ctx.Id}{(ctx.Flag == 1 ? "cmp" : "run")}",
                Type = ctx.Flag == 1 ? "compare" : "run",
            });
        }

        private async Task LoadSubmissionsAsync(ImportContext ctx, ZipArchive zip)
        {
            const string prefix = "submissions/";
            List<ZipArchiveEntry> files = zip.Entries
                .Where(z => z.FullName.StartsWith(prefix) && !z.FullName.EndsWith('/'))
                .ToList();
            List<Language> langs = await Facade.Languages.ListAsync();

            foreach (ZipArchiveEntry file in files)
            {
                if (file.Length > 65536)
                {
                    Log($"Too big for jury solution '{file.FullName}'");
                    continue;
                }

                string fileExt = Path.GetExtension(file.FullName);
                if (fileExt == ".cc") fileExt = ".cpp";
                Language lang = langs.FirstOrDefault(l => "." + l.FileExtension == fileExt);

                if (lang == null)
                {
                    Log($"No language found for jury solution '{file.FullName}'.");
                    continue;
                }

                Verdict expected = Verd.GetValueOrDefault(file.FullName.Split('/')[1]);
                string code = await file.ReadAsStringAsync();
                Submission sub = await ctx.SubmitAsync(code, lang.Id, expected);
                Log($"Jury solution '{file.FullName}' saved s{sub.Id}.");
            }
        }

        private IReadOnlyList<(string, string)> GetTexContent(string content)
        {
            return new[] { ("description", content) };
        }

        public override async Task<List<Problem>> ImportAsync(
            Stream stream,
            string uploadFileName,
            string username)
        {
            using ZipArchive zipArchive = new(stream);

            ImportContext ctx = await CreateAsync(new Problem
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
                Executable exec = await GetOutputValidatorAsync(ctx, zipArchive);
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
            bool hasMarkdownStatement = false;
            foreach (string mdfile in ResourceDictionary.MarkdownFiles)
            {
                ZipArchiveEntry entry = zipArchive.GetEntry("problem_statement/" + mdfile + ".md");
                if (entry == null) continue;

                if (mdfile == "description") hasMarkdownStatement = true;
                string mdcontent = await entry.ReadAsStringAsync();

                string tags = $"p{ctx.Id}";
                string content = await Files.ImportWithImagesAsync(Markdown, mdcontent, tags);
                await ctx.WriteStatementSectionAsync(mdfile, content);

                Log($"Adding statement section 'problem_statement/{mdfile}.md'.");
            }

            if (!hasMarkdownStatement)
            {
                foreach (string texfile in TexStmt)
                {
                    ZipArchiveEntry entry = zipArchive.GetEntry("problem_statement/" + texfile);
                    if (entry == null) continue;

                    string texcontent = await entry.ReadAsStringAsync();
                    IReadOnlyList<(string, string)> contents = GetTexContent(texcontent);
                    foreach (var (fn, ct) in contents) await ctx.WriteStatementSectionAsync(fn, ct);

                    Log($"Adding statement section 'problem_statement/{texfile}'.");
                    break;
                }
            }

            // Load testcases
            await ctx.KattisAsync(zipArchive, "data/sample/", false);
            await ctx.KattisAsync(zipArchive, "data/secret/", true);
            Log("All testcases has been added.");

            // Load submissions
            await LoadSubmissionsAsync(ctx, zipArchive);
            Log("All jury solutions has been added.");

            Problem problem = await ctx.FinalizeAsync();
            return new List<Problem> { problem };
        }
    }
}
