using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace Xylab.Polygon.TagHelpers
{
    [HtmlTargetElement("ace")]
    public class AceEditorTagHelper : XysTagHelper
    {
        [HtmlAttributeName("file")]
        public string File { get; set; }

        [HtmlAttributeName("value")]
        public string Content { get; set; }

        [HtmlAttributeNotBound]
        public string RandomId { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            RandomId = "editor" + Guid.NewGuid().ToString().Substring(0, 6);

            var e = new IndentHtmlContentBuilderWrapper(output.Content);

            e.L().H("<div class=\"editor\" id=\"").Encode(RandomId).H("\" style=\"font-size:0.8em\">")
                .Encode(Content)
                .H("</div>");

            e.L().H("<script> $(function () {");

            using (e.Indent())
            {
                e.L().H("var ").H(RandomId).H(" = ace.edit('").H(RandomId).H("');");
                e.L().H(RandomId).H(".setTheme('ace/theme/eclipse');");
                e.L().H(RandomId).H(".setOptions({ maxLines: Infinity });");
                e.L().H(RandomId).H(".setReadOnly(true);");
                e.L().H("var modelist = ace.require('ace/ext/modelist');");
                e.L().H("var filePath = '").H(File).H("';");
                e.L().H("var mode = modelist.getModeForPath(filePath).mode;");
                e.L().H(RandomId).H(".getSession().setMode(mode);");
                e.L().H("document.getElementById('").H(RandomId).H("').editor = ").H(RandomId).H(";");
            }

            e.L().H("}); </script>");
        }
    }
}
