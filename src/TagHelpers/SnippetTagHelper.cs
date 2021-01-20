using Microsoft.AspNetCore.Razor.TagHelpers;
using Polygon.Storages;

namespace Polygon.TagHelpers
{
    [HtmlTargetElement("snippet")]
    public class SnippetTagHelper : JudgingResultTagHelperBase
    {
        public SnippetTagHelper(IJudgingStore judgingStore)
            : base(judgingStore, 1024)
        {
        }

        protected override void Render(TagHelperOutput output, string source, int? truncatedLength)
        {
            output.TagName = "pre";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.AddClass("output_text");
            output.Content.Append(source);

            if (truncatedLength.HasValue)
            {
                output.Content.Append($"...\n[content display truncated after {truncatedLength}B]");
            }
        }
    }
}
