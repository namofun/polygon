using Microsoft.Extensions.FileProviders;
using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// The <see cref="IActionResult"/> for output an file as base64 results.
    /// </summary>
    public class Base64StreamResult : ActionResult
    {
        public IFileInfo FileInfo { get; set; }

        public Base64StreamResult(IFileInfo file) => FileInfo = file;

        const int byteLen = 1024 * 3 * 256;
        const int charLen = 1024 * 4 * 256;
        private static readonly ArrayPool<byte> opts = ArrayPool<byte>.Create(byteLen, 16);
        private static readonly ArrayPool<char> ress = ArrayPool<char>.Create(charLen, 16);

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.StatusCode = 200;
            response.ContentType = "application/json";
            response.ContentLength = (FileInfo.Length + 2) / 3 * 4 + 2;
            using var f1 = FileInfo.CreateReadStream();

            byte[] opt = opts.Rent(byteLen);
            char[] res = ress.Rent(charLen);
            var sw = new StreamWriter(response.Body);
            await sw.WriteAsync('"');

            while (true)
            {
                int len = await f1.ReadAsync(opt, 0, byteLen);
                if (len == 0) break;
                int len2 = Convert.ToBase64CharArray(opt, 0, len, res, 0);
                await sw.WriteAsync(res, 0, len2);
            }

            await sw.WriteAsync('"');
            await sw.DisposeAsync();
            opts.Return(opt);
            ress.Return(res);
        }
    }
}
