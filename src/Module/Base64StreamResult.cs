using Microsoft.Extensions.FileProviders;
using System;
using System.Buffers;
using System.Buffers.Text;
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
        private static readonly ArrayPool<byte> ress = ArrayPool<byte>.Create(charLen, 16);
        private static readonly byte[] qoute = System.Text.Encoding.UTF8.GetBytes("\"");

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            var cancellationToken = context.HttpContext.RequestAborted;
            response.StatusCode = 200;
            response.ContentType = "application/json";
            response.ContentLength = (FileInfo.Length + 2) / 3 * 4 + 2;
            using var f1 = FileInfo.CreateReadStream();

            byte[] opt = opts.Rent(byteLen);
            byte[] res = ress.Rent(charLen);
            await response.BodyWriter.WriteAsync(qoute, cancellationToken);
            long left = FileInfo.Length;

            while (left > 0)
            {
                int readLen = left > byteLen ? byteLen : checked((int)left);
                for (int len = 0; len < readLen; )
                    len += await f1.ReadAsync(opt, len, readLen - len);
                var s = Base64.EncodeToUtf8(opt.AsSpan(0, readLen), res, out int len1, out int len2, true);
                if (s != OperationStatus.Done || len1 != readLen)
                    throw new InvalidOperationException();
                await response.BodyWriter.WriteAsync(res.AsMemory(0, len2), cancellationToken);
                left -= readLen;
            }

            await response.BodyWriter.WriteAsync(qoute, cancellationToken);
            opts.Return(opt);
            ress.Return(res);
        }
    }
}
