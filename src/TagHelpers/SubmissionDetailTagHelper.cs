using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;
using Xylab.Polygon.Models;

namespace Xylab.Polygon.TagHelpers
{
    [HtmlTargetElement("submission-detail", TagStructure = TagStructure.WithoutEndTag)]
    public class SubmissionDetailTagHelper : XysTagHelper
    {
        [HtmlAttributeName("model")]
        public ISubmissionDetail Model { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeNotBound]
        public IViewComponentHelper Component { get; }

        public SubmissionDetailTagHelper(IViewComponentHelper htmlHelper)
        {
            Component = htmlHelper;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            ((IViewContextAware)Component).Contextualize(ViewContext);
            output.TagName = null;
            output.Content.SetHtmlContent(await Component.InvokeAsync("SubmissionDetail", Model));
        }
    }
}
