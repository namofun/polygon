using Markdig;
using Microsoft.Extensions.FileProviders;
using Polygon.Entities;
using Polygon.Storages;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace Polygon.Packaging
{
    public sealed class KattisExportProvider : IExportProvider
    {
        private class ExportSubmission
        {
            public Verdict ExpectedResult { get; }
            public string FileExtension { get; }
            public byte[] SourceCode { get; }
            public int Id { get; }

            public ExportSubmission(int id, string source, string fileExt, Verdict? expected)
            {
                Id = id;
                SourceCode = Convert.FromBase64String(source);
                FileExtension = fileExt;
                ExpectedResult = expected ?? Verdict.Unknown;
            }
        }

        private static readonly IReadOnlyDictionary<Verdict, string> KattisVerdict
            = new Dictionary<Verdict, string>
            {
                [Verdict.Accepted] = "accepted",
                [Verdict.WrongAnswer] = "wrong_answer",
                [Verdict.TimeLimitExceeded] = "time_limit_exceeded",
                [Verdict.RuntimeError] = "run_time_error",
                [Verdict.MemoryLimitExceeded] = "run_time_error",
            };

        private IMarkdownService Markdown { get; }
        private IPolygonFacade Facade { get; }
        private IWwwrootFileProvider Files { get; }

        public KattisExportProvider(IPolygonFacade facade, IMarkdownService markdown, IWwwrootFileProvider files)
        {
            Facade = facade;
            Markdown = markdown;
            Files = files;
        }

        public async Task<ExportResult> ExportAsync(Problem problem)
        {
            var memStream = new MemoryStream();
            var zip = new ZipArchive(memStream, ZipArchiveMode.Create, true);

            // Export the testcases.
            var tcs = await Facade.Testcases.ListAsync(problem.Id);

            foreach (var tc in tcs)
            {
                var prefix = $"data/{(tc.IsSecret ? "secret" : "sample")}/{tc.Rank}";

                var inputFile = await Facade.Testcases.GetInputAsync(tc);
                using (var inputFile2 = inputFile.CreateReadStream())
                    await zip.CreateEntryFromStream(inputFile2, prefix + ".in");

                var outputFile = await Facade.Testcases.GetOutputAsync(tc);
                using (var outputFile2 = outputFile.CreateReadStream())
                    await zip.CreateEntryFromStream(outputFile2, prefix + ".ans");

                if (tc.Description != $"{tc.Rank}")
                    zip.CreateEntryFromString(tc.Description, prefix + ".desc");

                if (tc.Point != 0)
                    zip.CreateEntryFromString($"{tc.Point}", prefix + ".point");
            }

            // Export the submissions.
            var subs = await Facade.Submissions.ListAsync(
                s => new ExportSubmission(s.Id, s.SourceCode, s.l.FileExtension, s.ExpectedResult),
                s => s.ProblemId == problem.Id && s.ExpectedResult != null);

            foreach (var sub in subs)
            {
                string result = KattisVerdict.GetValueOrDefault(sub.ExpectedResult, "ignore")!;
                zip.CreateEntryFromByteArray(
                    content: sub.SourceCode,
                    entry: $"submissions/{result}/s{sub.Id}.{sub.FileExtension}");
            }

            // Export the executables.
            var execs = new List<Executable>();
            if (problem.CompareScript != "compare")
                execs.Add(await Facade.Executables.FindAsync(problem.CompareScript));
            if (problem.RunScript != "run")
                execs.Add(await Facade.Executables.FindAsync(problem.RunScript));

            foreach (var exec in execs)
            {
                var subdir = $"output_validators/{exec.Id}/";
                using var itt = new MemoryStream(exec.ZipFile!);
                using var zpp = new ZipArchive(itt);

                foreach (var ent in zpp.Entries)
                {
                    using var rds = ent.Open();
                    var entry = await zip.CreateEntryFromStream(rds, Path.Combine(subdir, ent.Name));
                    entry.ExternalAttributes = ent.ExternalAttributes;
                }
            }

            // Export the statements.
            foreach (var mdname in ResourceDictionary.MarkdownFiles)
            {
                var file = await Facade.Problems.GetFileAsync(problem, $"{mdname}.md");
                if (!file.Exists) continue;
                string mdContent = (await file.ReadAsync())!;

                string news = await Files.ExportWithImagesAsync(Markdown, mdContent);
                zip.CreateEntryFromString(news, $"problem_statement/{mdname}.md");
            }

            // Export the problem.yaml.
            var sb = new StringBuilder();
            sb.AppendLine("name: " + problem.Title);
            if (!string.IsNullOrEmpty(problem.Source))
                sb.AppendLine("source: " + problem.Source);
            sb.AppendLine();
            sb.AppendLine("limits:");
            sb.AppendLine("    time: " + (problem.TimeLimit / 1000.0));
            sb.AppendLine("    memory: " + (problem.MemoryLimit / 1024));
            if (problem.OutputLimit != 4096)
                sb.AppendLine("    output: " + (problem.OutputLimit / 1024));
            sb.AppendLine();
            if (!string.IsNullOrEmpty(problem.ComapreArguments))
                sb.AppendLine("validator_flags: " + problem.ComapreArguments);
            if (problem.RunScript != "run")
                sb.AppendLine("validation: custom interactive");
            else if (problem.CompareScript != "compare")
                sb.AppendLine("validation: custom");
            zip.CreateEntryFromString(sb.ToString(), "problem.yaml");

            // Export domjudge-problem.ini.
            zip.CreateEntryFromString(
                content: $"timelimit = {problem.TimeLimit / 1000.0}\n",
                entry: "domjudge-problem.ini");

            zip.Dispose();
            memStream.Position = 0;
            return new ExportResult($"p{problem.Id}.zip", "application/zip", memStream);
        }
    }
}
