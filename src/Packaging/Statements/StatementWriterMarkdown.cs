using Markdig;
using Markdig.Renderers.LaTeX;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using Xylab.Polygon.Models;

namespace Xylab.Polygon.Packaging
{
    public class MarkdownStatementWriter : IStatementWriter
    {
        private IMarkdownService Markdown { get; }
        private HtmlEncoder Encoder { get; }
        private IWwwrootFileProvider Files { get; }

        public MarkdownStatementWriter(IMarkdownService markdownService, HtmlEncoder encoder, IWwwrootFileProvider files)
        {
            Markdown = markdownService;
            Encoder = encoder;
            Files = files;
        }

        public static readonly Dictionary<string, string> MarkdownReplacements
            = new()
            {
                [" -- "] = " — ",
                [" --- "] = " — ",
                [" ---- "] = " — ",
            };

        public void BuildHtml(StringBuilder htmlBuilder, Statement statement)
        {
            string RenderMarkdown(string source, Action<MarkdownDocument> options = null)
            {
                var document = Markdown.Parse(source);
                options?.Invoke(document);
                var result = Markdown.RenderAsHtml(document);
                foreach (var (key, value) in MarkdownReplacements)
                {
                    result = result.Replace(key, value);
                }

                return result;
            }

            htmlBuilder.AppendLine($"<h1>{statement.Problem.Title}</h1>");

            htmlBuilder.AppendLine("<ul class=\"list-unstyled\">");
            htmlBuilder.AppendLine($"  <li>Time limit: {statement.Problem.TimeLimit}ms</li>");
            htmlBuilder.AppendLine($"  <li>Memory limit: {statement.Problem.MemoryLimit}k</li>");
            htmlBuilder.AppendLine("</ul>");
            htmlBuilder.AppendLine();

            htmlBuilder.AppendLine("<div id=\"problem-descibe\">");

            if (!string.IsNullOrEmpty(statement.Description))
            {
                htmlBuilder.AppendLine("<h3>Description</h3>");
                htmlBuilder.AppendLine(RenderMarkdown(statement.Description));
            }

            if (!string.IsNullOrEmpty(statement.Input))
            {
                htmlBuilder.AppendLine("<h3>Input</h3>");
                htmlBuilder.AppendLine(RenderMarkdown(statement.Input));
            }

            if (!string.IsNullOrEmpty(statement.Output))
            {
                htmlBuilder.AppendLine("<h3>Output</h3>");
                htmlBuilder.AppendLine(RenderMarkdown(statement.Output));
            }

            if (!string.IsNullOrEmpty(statement.Interaction))
            {
                htmlBuilder.AppendLine("<h3>Interaction Protocol</h3>");
                htmlBuilder.AppendLine(RenderMarkdown(statement.Interaction));
            }

            if (statement.Samples.Count > 0)
            {
                htmlBuilder.AppendLine("<h3>Sample</h3>");

                foreach (var item in statement.Samples)
                {
                    htmlBuilder.AppendLine("<div class=\"samp\">");

                    if (!string.IsNullOrEmpty(item.Input))
                    {
                        htmlBuilder.AppendLine("<div class=\"input\">");
                        htmlBuilder.AppendLine("<div class=\"title\">Input</div>");
                        htmlBuilder.Append("<pre>").Append(Encoder.Encode(item.Input)).AppendLine("</pre>");
                        htmlBuilder.AppendLine("</div>");
                    }

                    htmlBuilder.AppendLine("<div class=\"output\">");
                    htmlBuilder.AppendLine("<div class=\"title\">Output</div>");
                    htmlBuilder.Append("<pre>").Append(Encoder.Encode(item.Output)).AppendLine("</pre>");
                    htmlBuilder.AppendLine("</div>");
                    htmlBuilder.AppendLine("</div>");
                    htmlBuilder.AppendLine();
                }
            }

            if (!string.IsNullOrEmpty(statement.Hint))
            {
                htmlBuilder.AppendLine("<h3>Hint</h3>");
                htmlBuilder.AppendLine(RenderMarkdown(statement.Hint));
                htmlBuilder.AppendLine();
            }

            htmlBuilder.AppendLine("</div>");
        }

        public void BuildLatex(ZipArchive zip, Statement statement, string filePrefix = "")
        {
            string ExtendUrl(string url)
            {
                if (url.StartsWith("/images/problem/"))
                {
                    IFileInfo file = Files.GetFileInfo(url.TrimStart('/'));
                    if (!file.Exists) return url;
                    var ext = Path.GetExtension(file.PhysicalPath).TrimStart('.');
                    var guid = Guid.NewGuid().ToString("N")[..16];
                    var fileName = $"{guid}.{ext}";
                    zip.CreateEntryFromFile(file.PhysicalPath, filePrefix + fileName);
                    return Path.GetFileNameWithoutExtension(fileName) + Path.GetExtension(fileName);
                }
                else if (url.StartsWith("data:image/"))
                {
                    var index = url.IndexOf(";base64,");
                    if (index == -1) return url;
                    string ext = url[11..index];
                    var guid = Guid.NewGuid().ToString("N")[..16];
                    var fileName = $"{guid}.{ext}";

                    try
                    {
                        var fileIn = Convert.FromBase64String(url[(index + 8)..]);
                        zip.CreateEntryFromByteArray(fileIn, filePrefix + fileName);
                        return Path.GetFileNameWithoutExtension(fileName) + Path.GetExtension(fileName);
                    }
                    catch
                    {
                        return url;
                    }
                }

                return url;
            }

            using var texWriter = new StringWriter { NewLine = "\n" };
            var problem = statement.Problem;
            texWriter.Write($"\\begin{{problem}}{{{problem.Title}}}");
            texWriter.Write($"{{standard input}}{{standard output}}");
            double timeLimit = problem.TimeLimit / 1000.0;
            texWriter.Write($"{{{timeLimit} second{(timeLimit > 1 ? "s" : "")}}}");
            texWriter.WriteLine($"{{{problem.MemoryLimit / 1024} megabytes}}");
            texWriter.WriteLine();

            var renderer = new LatexRenderer(texWriter);
            Markdown.Pipeline.Setup(renderer);

            void GoRender(string opt)
            {
                var document = Markdown.Parse(opt);

                document.Transverse<LinkInline>(o =>
                {
                    if (o.IsImage) o.Url = ExtendUrl(o.Url);
                });

                renderer.Render(document);
                renderer.EnsureLine().WriteLine().WriteLine();
            }

            GoRender(statement.Description);

            if (!string.IsNullOrWhiteSpace(statement.Input))
            {
                renderer.WriteLine("\\InputFile").WriteLine();
                GoRender(statement.Input);
            }

            if (!string.IsNullOrWhiteSpace(statement.Output))
            {
                renderer.WriteLine("\\OutputFile").WriteLine();
                GoRender(statement.Output);
            }

            if (!string.IsNullOrWhiteSpace(statement.Interaction))
            {
                renderer.WriteLine("\\Interaction").WriteLine();
                GoRender(statement.Interaction);
            }

            if (statement.Samples.Count > 0)
            {
                renderer.WriteLine("\\Example").WriteLine();
                renderer.WriteLine("\\begin{example}");

                for (int i = 0; i < statement.Samples.Count; i++)
                {
                    zip.CreateEntryFromString(statement.Samples[i].Input, $"{filePrefix}example.{i + 1}.in");
                    zip.CreateEntryFromString(statement.Samples[i].Output, $"{filePrefix}example.{i + 1}.ans");
                    renderer.WriteLine($"\\exmpfile{{example.{i + 1}.in}}{{example.{i + 1}.ans}}%");
                }

                renderer.WriteLine("\\end{example}");
                renderer.WriteLine();
            }

            if (!string.IsNullOrWhiteSpace(statement.Hint))
            {
                renderer.WriteLine("\\Notes").WriteLine();
                GoRender(statement.Hint);
            }

            texWriter.WriteLine("\\end{problem}");
            zip.CreateEntryFromString(texWriter.ToString(), $"{filePrefix}problem.tex");
        }
    }
}
