﻿using Microsoft.AspNetCore.Razor.TagHelpers;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon.TagHelpers
{
    /// <summary>
    /// Show badge icons for verdict.
    /// </summary>
    [HtmlTargetElement("verdict-badge")]
    public class VerdictBadgeTagHelper : XysTagHelper
    {
        [HtmlAttributeName("value")]
        public Verdict Value { get; set; }

        [HtmlAttributeName("judging-pending")]
        public bool IsJudgingPending { get; set; }

        [HtmlAttributeName("tooltip")]
        public string TooltipTitle { get; set; }

        static readonly (string, string)[] st = new[]
        {
            ("secondary", "?"), // 0
            ("danger", "t"),
            ("danger", "m"),
            ("danger", "r"),
            ("danger", "o"),
            ("danger", "w"), // 5
            ("secondary", "c"),
            ("danger", "w"),
            ("secondary", "?"),
            ("info", "?"),
            ("warning", "?"), // 10
            ("success", "✓"),
        };

        private (string, string) SolveAsVerdict()
        {
            var v = (int)Value;
            return  IsJudgingPending && v == (int)Verdict.Pending
                ? ("verdict-sm badge badge-primary", st[v].Item2)
                : ("verdict-sm badge badge-" + st[v].Item1, st[v].Item2);
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            output.TagMode = TagMode.StartTagAndEndTag;
            var (@class, content) = SolveAsVerdict();

            output.Attributes.TryGetAttribute("class", out var clv);
            output.Attributes.SetAttribute("class", (clv?.Value ?? "") + " " + @class);

            if (!string.IsNullOrEmpty(TooltipTitle))
            {
                output.Attributes.SetAttribute("data-toggle", "tooltip");
                output.Attributes.SetAttribute("title", TooltipTitle);
            }

            output.Content.AppendHtml(content);
        }
    }
}
