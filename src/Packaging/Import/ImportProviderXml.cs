using Markdig;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Polygon.Entities;
using Polygon.Models;
using Polygon.Storages;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Polygon.Packaging
{
    public sealed class XmlImportProvider : ImportProviderBase
    {
        public IMarkdownService Markdown { get; }
        private IWwwrootFileProvider Files { get; }

        static readonly Dictionary<string, string> nodes = new Dictionary<string, string>
        {
            ["description"] = "description",
            ["input"] = "inputdesc",
            ["output"] = "outputdesc",
            ["hint"] = "hint",
        };

        static readonly (string, bool)[] testcaseGroups = new[] { ("samples", false), ("test_cases", true) };

        public XmlImportProvider(
            IPolygonFacade facade,
            ILogger<XmlImportProvider> logger,
            IMarkdownService markdown,
            IWwwrootFileProvider files)
            : base(facade, logger)
        {
            Markdown = markdown;
            Files = files;
        }

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

            XElement doc = document.Root;

            ImportContext ctx = await CreateAsync(new Problem
            {
                Title = doc.Element("title").Value,
                MemoryLimit = int.Parse(doc.Element("memory_limit").Value),
                TimeLimit = int.Parse(doc.Element("time_limit").Value),
                AllowJudge = false,
                AllowSubmit = false,
                CompareScript = "compare",
                RunScript = "run",
                OutputLimit = 4096,
                Source = doc.Element("author")?.Value ?? username,
            });

            // Write all markdown files into folders.
            foreach (var (nodeName, fileName) in nodes)
            {
                var element = doc.Element(nodeName);
                if (string.IsNullOrEmpty(element.Value)) continue;
                string mdcontent = element.Value;
                var tags = $"p{ctx.Id}";
                string content = await Files.ImportWithImagesAsync(Markdown, mdcontent, tags);
                await ctx.WriteStatementSectionAsync(fileName, content);
            }

            // Add testcases.
            foreach (var (tcgName, isSecret) in testcaseGroups)
            {
                foreach (XElement testcase in doc.Element(tcgName).Elements())
                {
                    await ctx.AddAsync(
                        new MemoryTestcase(
                            desc: (string)testcase.Element("desc"),
                            input: (string)testcase.Element("input"),
                            output: (string)testcase.Element("output"),
                            point: (int)testcase.Element("point")),
                        isSecret);
                }
            }

            Problem problem = await ctx.FinalizeAsync();
            return new List<Problem> { problem };
        }
    }
}
