using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Models;
using Xylab.Polygon.Storages;

namespace Xylab.Polygon.Packaging
{
    public sealed class FpsImportProvider : ImportProviderBase
    {
        public FpsImportProvider(
            IPolygonFacade facade,
            ILogger<FpsImportProvider> logger)
            : base(facade, logger)
        {
        }

        static readonly Dictionary<string, string> nodes = new()
        {
            ["description"] = "description",
            ["input"] = "inputdesc",
            ["output"] = "outputdesc",
            ["hint"] = "hint",
        };

        static readonly (string, bool, string)[] testcaseGroups = new[]
        {
            ("sample_input", false, "sample_output"),
            ("test_input", true, "test_output")
        };

        public override async Task<List<Problem>> ImportAsync(
            Stream stream,
            string uploadFileName,
            string username)
        {
            XDocument document;

            using (StreamReader sr = new(stream))
            {
                string content = await sr.ReadToEndAsync();
                document = XDocument.Parse(content);
            }

            XElement doc2 = document.Root;
            List<Problem> probs = new();
            List<Language> langs = await Facade.Languages.ListAsync();

            foreach (XElement doc in doc2.Elements("item"))
            {
                ImportContext ctx = await CreateAsync(new Problem
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
                    string content = doc.Element(nodeName)?.Value;
                    if (string.IsNullOrEmpty(content)) continue;
                    await ctx.WriteStatementSectionAsync(fileName, content);
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
                foreach (XElement submission in doc.Elements("solution"))
                {
                    string langName = submission.Attribute("language").Value;
                    Language lang = langs.FirstOrDefault(l => l.Name == langName);
                    if (lang == null) lang = langs.FirstOrDefault();

                    string content = submission.Value;
                    Submission s = await ctx.SubmitAsync(content, lang.Id, Verdict.Unknown);

                    Log($"Submission s{s.Id} created.");
                }

                probs.Add(await ctx.FinalizeAsync());
            }

            return probs;
        }
    }
}
