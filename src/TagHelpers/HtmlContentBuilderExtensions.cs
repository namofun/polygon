using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.IO;
using System.Text.Encodings.Web;

namespace Xylab.Polygon.TagHelpers
{
    public class IndentHtmlContentBuilderWrapper : IHtmlContentBuilder, IDisposable
    {
        private readonly TagHelperContent _content;
        private int _count;

        public IndentHtmlContentBuilderWrapper(TagHelperContent content)
        {
            _content = content;
            _count = 0;
        }

        void IDisposable.Dispose()
        {
            _count--;
        }

        public IndentHtmlContentBuilderWrapper Dispose()
        {
            _count--;
            return this;
        }

        public IndentHtmlContentBuilderWrapper Indent()
        {
            _count++;
            return this;
        }

        public IndentHtmlContentBuilderWrapper L()
        {
            _content.AppendHtml("\n");
            for (int i = 0; i < _count; i++) _content.AppendHtml("    ");
            return this;
        }

        public IndentHtmlContentBuilderWrapper H(string encoded)
        {
            _content.AppendHtml(encoded);
            return this;
        }

        public IndentHtmlContentBuilderWrapper Encode(string unencoded)
        {
            _content.Append(unencoded);
            return this;
        }

        public IndentHtmlContentBuilderWrapper H(int encoded)
        {
            _content.AppendHtml(encoded.ToString());
            return this;
        }

        public IndentHtmlContentBuilderWrapper H(double encoded)
        {
            _content.AppendHtml(encoded.ToString());
            return this;
        }

        IHtmlContentBuilder IHtmlContentBuilder.Append(string unencoded) => _content.Append(unencoded);
        IHtmlContentBuilder IHtmlContentBuilder.AppendHtml(IHtmlContent content) => _content.AppendHtml(content);
        IHtmlContentBuilder IHtmlContentBuilder.AppendHtml(string encoded) => _content.AppendHtml(encoded);
        IHtmlContentBuilder IHtmlContentBuilder.Clear() => _content.Clear();
        void IHtmlContentContainer.CopyTo(IHtmlContentBuilder builder) => _content.CopyTo(builder);
        void IHtmlContentContainer.MoveTo(IHtmlContentBuilder builder) => _content.MoveTo(builder);
        void IHtmlContent.WriteTo(TextWriter writer, HtmlEncoder encoder) => _content.WriteTo(writer, encoder);
    }
}
