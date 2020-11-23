using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace Polygon.TagHelpers
{
    /// <summary>
    /// Text icon for misc things
    /// </summary>
    [HtmlTargetElement("misc-status")]
    public class MiscStatusTagHelper : XysTagHelper
    {
        public enum UseType
        {
            None,
            TeamStatus,
            RegistrationStatus,
            ContestRule,
        }

        [HtmlAttributeName("type")]
        public UseType Type { get; set; }

        [HtmlAttributeName("value")]
        public int Value { get; set; }

        [HtmlAttributeName("tooltip")]
        public string TooltipTitle { get; set; }

        private (string, string) SolveAsTeamStatus()
        {
            return Value switch
            {
                0 => ("sol sol_queued", "pending"),
                1 => ("sol sol_correct", "accepted"),
                2 => ("sol sol_incorrect", "rejected"),
                3 => ("sol sol_incorrect", "deleted"),
                _ => ("sol sol_queued", "unknown"),
            };
        }

        private (string, string) SolveAsRegistrationStatus()
        {
            return Value switch
            {
                0 => ("sol sol_incorrect", "closed"),
                _ => ("sol sol_correct", "open"),
            };
        }

        private (string, string) SolveAsRule()
        {
            return Value switch
            {
                0 => ("sol", "XCPC"),
                1 => ("sol", "CF"),
                2 => ("sol", "IOI"),
                _ => ("sol sol_incorrect", "UKE"),
            };
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            output.TagMode = TagMode.StartTagAndEndTag;

            var (@class, content) = Type switch
            {
                UseType.TeamStatus => SolveAsTeamStatus(),
                UseType.RegistrationStatus => SolveAsRegistrationStatus(),
                UseType.ContestRule => SolveAsRule(),
                _ => throw new InvalidOperationException(),
            };

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
