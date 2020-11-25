using Microsoft.Extensions.Logging;
using Polygon.Entities;
using Polygon.Storages;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Polygon.Packaging
{
    public sealed class CodeforcesImportProvider : ImportProviderBase
    {
        const int LINUX644 = -2119958528;

        private static readonly IReadOnlyDictionary<string, Verdict> Verds =
            new Dictionary<string, Verdict>
            {
                ["accepted"] = Verdict.Accepted,
                ["main"] = Verdict.Accepted,
                ["time-limit-exceeded-or-accepted"] = Verdict.TimeLimitExceeded,
                ["time-limit-exceeded"] = Verdict.TimeLimitExceeded,
                ["time-limit-exceeded-or-memory-limit-exceeded"] = Verdict.TimeLimitExceeded,
                ["memory-limit-exceeded"] = Verdict.MemoryLimitExceeded,
                ["rejected"] = Verdict.RuntimeError,
                ["failed"] = Verdict.RuntimeError,
                ["wrong-answer"] = Verdict.WrongAnswer,
            };

        private static readonly IReadOnlyDictionary<string, string?> Checkers =
            new Dictionary<string, string?>
            {
                ["504168fb5f80beb55d90d453633b50ff"] = "case_sensitive space_change_sensitive",
                ["c64791ffeb412ceb0602d51e86eb220d"] = "float_tolerance 1e-4",
                ["1ed169c6f859507746ddc061f06ff7b2"] = "float_tolerance 1e-6",
                ["84e6c5cb24799378d1e04e95d6e961b2"] = "float_tolerance 1e-9",
                ["fd50c77a483254949a69bc07e83a056c"] = null,
                ["28cda0257b7aaedd6c348194b75a0447"] = null,
                ["0509bc8c7a3e4a2219d9ca39e5bf3ce8"] = null,
                ["dd628828076360fb6f12582864ed0625"] = null,
                ["fecbdb15ac0b93226fa74dab3169da9d"] = null,
                ["8e03582d85b4f398f6b36f8dd17fc5a5"] = null,
                ["d6deb0e3c0ae8cd4369d9bf54078a548"] = null,
            };

        private static readonly IReadOnlyDictionary<string, string> Names =
            new Dictionary<string, string>
            {
                ["description"] = "legend.tex",
                ["inputdesc"] = "input.tex",
                ["outputdesc"] = "output.tex",
                ["hint"] = "notes.tex",
                ["interact"] = "interaction.tex",
            };

        public CodeforcesImportProvider(IPolygonFacade facade, ILogger<CodeforcesImportProvider> logger) : base(facade, logger)
        {
        }

        private async Task<Executable> CreateExecutableAsync(
            ImportContext ctx, byte[] contents, string ext, bool cmp)
        {
            var execName = $"p{ctx.Id}{(cmp ? "cmp" : "run")}";

            var stream = new MemoryStream();
            using (var newzip = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                var f = newzip.CreateEntryFromByteArray(contents, "main" + ext);
                f.ExternalAttributes = LINUX644;
                using var testlib = ResourcesDictionary.GetTestlib();
                var f2 = await newzip.CreateEntryFromStream(testlib, "testlib.h");
                f2.ExternalAttributes = LINUX644;
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
                Id = execName,
                Type = cmp ? "compare" : "run",
            });
        }

        private async Task GetCheckerAsync(ImportContext ctx, XElement chks, ZipArchive zip)
        {
            var fileName = chks.Element("source").Attribute("path").Value;
            var entry = zip.GetEntry(fileName);
            using var stream = entry.Open();
            var content = new byte[entry.Length];
            int pos = 0;
            while (pos < entry.Length)
                pos += await stream.ReadAsync(content, pos, (int)entry.Length - pos);
            var md5 = content.ToMD5().ToHexDigest(true);

            string cmp = "compare"; string? args = null;
            if (Checkers.ContainsKey(md5)) args = Checkers[md5];
            else cmp = (await CreateExecutableAsync(ctx, content, Path.GetExtension(fileName), true)).Id;
        }

        private async Task GetInteractorAsync(ImportContext ctx, XElement iacs, ZipArchive zip)
        {
            if (iacs == null) return;
            var fileName = iacs.Element("source").Attribute("path").Value;
            var entry = zip.GetEntry(fileName);
            using var stream = entry.Open();

            var content = new byte[entry.Length];
            int pos = 0;
            while (pos < entry.Length)
                pos += await stream.ReadAsync(content, pos, (int)entry.Length - pos);
            var e = await CreateExecutableAsync(ctx, content, Path.GetExtension(fileName), false);
            ctx.Problem.RunScript = e.Id;
            ctx.Problem.CombinedRunCompare = true;
        }

        private async Task LoadSubmissionsAsync(ImportContext ctx, XElement sols, ZipArchive zip)
        {
            var langs = await Facade.Languages.ListAsync();
            foreach (var sol in sols.Elements("solution"))
            {
                var source = sol.Element("source");
                var fileName = source.Attribute("path").Value;
                var file = zip.GetEntry(fileName);
                var tag = sol.Attribute("tag").Value;

                var lang = langs.FirstOrDefault(l =>
                    "." + l.FileExtension == Path.GetExtension(file.FullName));

                if (lang == null)
                {
                    Log($"No language found for jury solution '{file.FullName}'.");
                }
                else
                {
                    var code = await file.ReadAsStringAsync();
                    var sub = await ctx.SubmitAsync(code, lang.Id, Verds.GetValueOrDefault(tag));
                    Log($"Jury solution '{file.FullName}' saved s{sub.Id}.");
                }
            }
        }

        private async Task LoadStatementAsync(ImportContext ctx, XElement name, ZipArchive zip)
        {
            if (name == null) return;
            var lang = name.Attribute("language").Value;
            ctx.Problem.Title = name.Attribute("value").Value;

            foreach (var (mdfile, filename) in Names)
            {
                var entry = zip.GetEntry($"statement-sections/{lang}/{filename}");
                if (entry == null) continue;
                string mdcontent = await entry.ReadAsStringAsync();
                await ctx.WriteAsync($"{mdfile}.md", mdcontent);
                Log($"Adding statement section 'statement-sections/{lang}/{filename}'.");
            }
        }

        private async Task LoadTestsetAsync(ImportContext ctx, XElement name, ZipArchive zip)
        {
            if (name == null) return;
            ctx.Problem.TimeLimit = int.Parse(name.Element("time-limit").Value);
            ctx.Problem.MemoryLimit = int.Parse(name.Element("memory-limit").Value) >> 10;
            var count = int.Parse(name.Element("test-count").Value);
            var testName = name.Attribute("name").Value;
            var tests = name.Element("tests").Elements("test").ToList();
            if (tests.Count != count)
                throw new InvalidDataException("Zip corrupt.");

            for (int i = 1; i <= count; i++)
            {
                var fileName = $"{testName}/{i:D2}";
                var test = tests[i-1];
                var attr1 = test.Attribute("description");
                var attr2 = test.Attribute("sample");

                var inp = zip.GetEntry(fileName);
                var outp = zip.GetEntry(fileName + ".a");

                if (inp == null || outp == null)
                {
                    Log($"Ignoring {fileName}.*");
                    continue;
                }

                var str = $"{i:D2}";
                if (attr1 != null)
                    str += ": " + attr1.Value;
                if (test.Attribute("method").Value == "generated")
                    str += "; " + test.Attribute("cmd").Value;

                await ctx.AddAsync(
                    input: (() => inp.Open(), inp.Length),
                    output: (() => outp.Open(), outp.Length),
                    str, 0, attr2?.Value != "true", $"{fileName}.{{,a}}");
            }
        }

        public override async Task<List<Problem>> ImportAsync(Stream stream, string uploadFileName, string username)
        {
            using var zipArchive = new ZipArchive(stream);
            var infoXml = zipArchive.GetEntry("problem.xml");
            var info = XDocument.Parse(await infoXml.ReadAsStringAsync()).Root;

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

            var names = info.Element("names").Elements("name");
            await LoadStatementAsync(ctx, names.FirstOrDefault(), zipArchive);

            var tests = info.Element("judging").Elements("testset");
            var testsCount = tests.Count();
            if (testsCount != 1)
                Log($"!!! Testset count is {testsCount}, using the first set...");
            await LoadTestsetAsync(ctx, tests.FirstOrDefault(), zipArchive);
            Log("All testcases has been added.");

            var assets = info.Element("assets");
            await LoadSubmissionsAsync(ctx, assets.Element("solutions"), zipArchive);
            Log("All jury solutions has been added.");

            await GetCheckerAsync(ctx, assets.Element("checker"), zipArchive);
            await GetInteractorAsync(ctx, assets.Element("interactor"), zipArchive);

            var problem = await ctx.FinalizeAsync();
            return new List<Problem> { problem };
        }
    }
}
