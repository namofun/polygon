using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using Xylab.Polygon.Storages;

namespace Xylab.Polygon.TagHelpers
{
    [HtmlTargetElement("interactive")]
    public class InteractiveLogTagHelper : JudgingResultTagHelperBase
    {
        public InteractiveLogTagHelper(IJudgingStore judgingStore)
            : base(judgingStore, 2000)
        {
        }

        protected override void Render(TagHelperOutput output, string source, int? truncatedLength)
        {
            output.TagName = "table";
            output.TagMode = TagMode.StartTagAndEndTag;
            var sb = output.Content;
            sb.AppendHtml("<tr><th>time</th><th>validator</th><th>submission<th></tr>\n");
            var log = source.AsSpan();

            int idx = 0;
            while (idx < log.Length)
            {
                int slashPos = log.Slice(idx).IndexOf('/');
                if (slashPos == -1) break; else slashPos += idx;
                string time = new string(log.Slice(idx + 1, slashPos - idx - 1));
                idx = slashPos + 1;
                int closePos = log.Slice(idx).IndexOf(']');
                if (closePos == -1) break; else closePos += idx;
                int len = int.Parse(log.Slice(idx, closePos - idx));
                if (closePos + 4 + len >= log.Length) break;
                idx = closePos + 1;
                bool is_validator = log[idx] == '>';

                sb.AppendHtml("<tr><td>").Append(time);
                if (!is_validator) sb.AppendHtml("</td><td>");
                sb.AppendHtml("</td><td class=\"output_text\">");
                var str = log.Slice(idx + 3, len); int igx;

                while ((igx = str.IndexOf('\n')) != -1)
                {
                    sb.Append(new string(str.Slice(0, igx))).Append("\u21B5").AppendHtml("<br/>");
                    str = str.Slice(igx + 1);
                }

                if (str.Length > 0) sb.Append(new string(str));
                if (is_validator) sb.AppendHtml("</td><td>");
                sb.AppendHtml("</td></tr>");
                idx += len + 4;
            }

            if (truncatedLength.HasValue)
            {
                sb.AppendHtml("<caption class=\"pb-0 pt-1 text-dark\">")
                    .Append($"[content display truncated after {truncatedLength}B]")
                    .AppendHtml("</caption>");
            }
        }
    }
}
