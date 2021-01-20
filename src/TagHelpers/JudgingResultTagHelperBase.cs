#nullable enable
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.FileProviders;
using Polygon.Storages;
using System;
using System.Buffers;
using System.Text;
using System.Threading.Tasks;

namespace Polygon.TagHelpers
{
    public abstract class JudgingResultTagHelperBase : XysTagHelper
    {
        private const string _deleted = "Record has been deleted.";
        private readonly IJudgingStore _judgingStore;
        private readonly int _readLength;

        [HtmlAttributeName("h5-title")]
        public string? Header5 { get; set; }

        [HtmlAttributeName("base64")]
        public string? Base64Source { get; set; }

        [HtmlAttributeName("nodata")]
        public string NoData { get; set; } = "There was no data.";

        [HtmlAttributeName("filename")]
        public string? FileName { get; set; }

        protected abstract void Render(TagHelperOutput output, string source, int? truncatedLength);

        public JudgingResultTagHelperBase(IJudgingStore judgingStore, int readLength)
        {
            _judgingStore = judgingStore;
            _readLength = readLength;
        }

        protected void ConvertBase64(string b64, out bool ok, out string content)
        {
            try
            {
                var values = Encoding.UTF8.GetString(Convert.FromBase64String(b64));
                ok = !string.IsNullOrWhiteSpace(values);
                content = ok ? values : NoData;
            }
            catch
            {
                ok = false;
                content = "Error while parsing " + b64 + " . Please contact XiaoYang.";
            }
        }

        protected async ValueTask<(bool, string, bool)> ReadFileAsync(IFileInfo file)
        {
            if (!file.Exists) return (false, _deleted, false);
            string content;

            using (var stream = file.CreateReadStream())
            using (var sr = new System.IO.StreamReader(stream))
            {
                var arr = ArrayPool<char>.Shared.Rent(_readLength);
                var len = await sr.ReadBlockAsync(arr, 0, _readLength);
                content = new string(arr, 0, len);
                ArrayPool<char>.Shared.Return(arr);
            }

            return string.IsNullOrWhiteSpace(content)
                ? (false, NoData, false)
                : (true, content, content.Length == _readLength);
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            string result = _deleted;
            bool ok = false, truncated = false;

            if (Base64Source != null)
            {
                ConvertBase64(Base64Source, out ok, out result);
            }
            else if (FileName != null)
            {
                var sec = FileName.Split('/');
                if (sec.Length != 3 ||
                    !int.TryParse(sec[0], out int judgingId) ||
                    !int.TryParse(sec[1], out int runId))
                {
                    result = $"Format of \"{FileName}\" is not correct.";
                }
                else
                {
                    var file = await _judgingStore.GetRunFileAsync(judgingId, runId, sec[2]);
                    (ok, result, truncated) = await ReadFileAsync(file);
                }
            }

            if (!ok)
            {
                output.TagName = "p";
                output.Attributes.AddClass("nodata");
                output.TagMode = TagMode.StartTagAndEndTag;
                output.Content.Append(result);
            }
            else
            {
                Render(output, result, truncated ? _readLength : default(int?));
            }

            if (!string.IsNullOrWhiteSpace(Header5))
            {
                output.PreElement.AppendHtml("<h5 class=\"pt-0\">").Append(Header5).AppendHtml("</h5>");
            }
        }
    }
}
