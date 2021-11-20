using Microsoft.Extensions.Logging;
using Polygon.Entities;
using Polygon.Models;
using Polygon.Storages;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Polygon.Packaging
{
    public sealed class FpsImportProvider : ImportProviderBase
    {
        public FpsImportProvider(IPolygonFacade facade, ILogger<FpsImportProvider> logger) : base(facade, logger)
        {
        }

        static readonly Dictionary<string, string> nodes = new()
        {
            ["description"] = "description.md",
            ["input"] = "inputdesc.md",
            ["output"] = "outputdesc.md",
            ["hint"] = "hint.md",
        };

        static readonly (string, bool, string)[] testcaseGroups = new[]
        {
            ("sample_input", false, "sample_output"),
            ("test_input", true, "test_output")
        };

        public override async Task<List<Problem>> ImportAsync(Stream stream, string uploadFileName, string username)
        {
            XDocument document;

            using (var sr = new StreamReader(stream))
            {
                var content = await sr.ReadToEndAsync();
                document = XDocument.Parse(content);
            }

            var doc2 = document.Root;
            var probs = new List<Problem>();
            var langs = await Facade.Languages.ListAsync();

            foreach (var doc in doc2.Elements("item"))
            {
                var ctx = await CreateAsync(new Problem
                {
                    Title = ((string)doc.Element("title")) ?? uploadFileName,
                    MemoryLimit = int.Parse(((string)doc.Element("memory_limit")) ?? "128") * 1024,
                    TimeLimit = int.Parse(((string)doc.Element("time_limit")) ?? "10") * 1000,
                    AllowJudge = false,
                    AllowSubmit = false,
                    CompareScript = "compare",
                    RunScript = "run",
                    OutputLimit = 4096,
                    Source = ((string)doc.Element("source")) ?? username,
                });

                // Write all markdown files into folders.
                foreach (var (nodeName, fileName) in nodes)
                {
                    var content = doc.Element(nodeName)?.Value;
                    if (string.IsNullOrEmpty(content)) continue;
                    await ctx.WriteAsync(fileName, content);
                }

                // Add testcases.
                int tot = 0;
                foreach (var (tcgName, isSecret, nextEleName) in testcaseGroups)
                foreach (XElement inputNode in doc.Elements(tcgName))
                {
                    if (inputNode.NextNode is not XElement outputNode || outputNode.Name != nextEleName)
                    {
                        Log($"Unknown node at {tot}.");
                        continue;
                    }

                    await ctx.AddAsync(
                        new MemoryTestcase($"{tot + 1}", (string)inputNode, (string)outputNode, 0),
                        isSecret);
                }

                // Add solutions
                foreach (var submission in doc.Elements("solution"))
                {
                    var langName = submission.Attribute("language").Value;
                    var lang = langs.FirstOrDefault(l => l.Name == langName);
                    if (lang == null) lang = langs.FirstOrDefault();

                    var content = submission.Value;
                    var s = await ctx.SubmitAsync(content, lang.Id, Verdict.Unknown);

                    Log($"Submission s{s.Id} created.");
                }

                probs.Add(await ctx.FinalizeAsync());
            }

            return probs;
        }
    }
}
