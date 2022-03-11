using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Xylab.Polygon.TagHelpers
{
    [HtmlTargetElement("nvd3", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class Nvd3TagHelper : XysTagHelper
    {
        [HtmlAttributeName("y-axis")]
        public string YAxis { get; set; }

        [HtmlAttributeName("id")]
        public string Id { get; set; }

        [HtmlAttributeName("title")]
        public string Title { get; set; }

        [HtmlAttributeName("baseline")]
        public double Baseline { get; set; }

        [HtmlAttributeName("max-value")]
        public double MaxValue { get; set; }

        [HtmlAttributeName("x-axis")]
        public string XAxis { get; set; }

        [HtmlAttributeName("key")]
        public string KeyName { get; set; }

        [HtmlAttributeName("data")]
        public IEnumerable<object> DataObject { get; set; }

        [HtmlAttributeNotBound]
        public JavaScriptEncoder JSEncoder { get; }

        public Nvd3TagHelper(JavaScriptEncoder encoder)
        {
            JSEncoder = encoder;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Add("style", "display: inline-block");
            output.Attributes.Add("id", Id);

            var generatedObject = JsonSerializer.Serialize(
                new { key = KeyName, values = DataObject },
                new JsonSerializerOptions { Encoder = JSEncoder });

            var e = new IndentHtmlContentBuilderWrapper(output.Content);

            e.L().H("<h3 id=\"graphs\">").Encode(Title).H("</h3>");
            e.L().H("<svg style=\"width:500px; height:250px;\"></svg>");

            e.L().H("<script> $(function () {");

            using (e.Indent())
            {
                e.L().H("var curdata = [").H(generatedObject).H("];");
                e.L().H("nv.addGraph(function () {");

                using (e.Indent())
                {
                    e.L().H("var chart = createNvd3Chart(").H(Math.Max(Baseline, MaxValue)).H(");");
                    e.L().H("chart.xAxis.axisLabel('").H(JSEncoder.Encode(XAxis)).H("');");
                    e.L().H("chart.yAxis.axisLabel('").H(JSEncoder.Encode(YAxis)).H("');");
                    e.L().H("d3.select('#").H(Id).H(" svg').datum(curdata).call(chart);");
                    e.L().H("var svgsize = chart.container.clientWidth || chart.container.parentNode.clientWidth;");
                    e.L().H("d3.select('#").H(Id).H(" svg').append('line').attr({")
                        .H("x1: chart.margin().left,")
                        .H("y1: chart.yAxis.scale()(").H(Baseline).H(") + chart.margin().top,")
                        .H("x2: +svgsize - chart.margin().right,")
                        .H("y2: chart.yAxis.scale()(").H(Baseline).H(") + chart.margin().top,")
                        .H("}).style('stroke', '#F00');");
                    e.L().H("nv.utils.windowResize(chart.update);");
                    e.L().H("return chart;");
                }

                e.L().H("});");
            }

            e.L().H("}); </script>");
        }
    }
}
