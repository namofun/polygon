using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Models;
using Polygon.Packaging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Extensions for ASP.NET Core MVC.
    /// </summary>
    internal static class FormFileExtensions
    {
        /// <summary>
        /// Read the form file content.
        /// </summary>
        /// <param name="file">The <see cref="IFormFile"/>.</param>
        /// <returns>The task for content bytes and MD5.</returns>
        public static async Task<(byte[], string)> ReadAsync(this IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var input = new byte[file.Length];
            int cursor = 0;
            while (cursor < file.Length)
                cursor += await stream.ReadAsync(input, cursor, input.Length - cursor);
            var inputHash = input.ToMD5().ToHexDigest(true);
            return (input, inputHash);
        }

        /// <summary>
        /// Get service of type <typeparamref name="T"/> from the <see cref="IServiceProvider"/>.
        /// </summary>
        /// <typeparam name="T">The type of service object to get.</typeparam>
        /// <param name="httpContext">The <see cref="IServiceProvider"/> to retrieve the service object from.</param>
        /// <returns>A service object of type <typeparamref name="T"/>.</returns>
        /// <exception cref="InvalidOperationException">There is no service of type <typeparamref name="T"/>.</exception>
        public static T GetService<T>(this HttpContext httpContext)
        {
            return httpContext.RequestServices.GetRequiredService<T>();
        }

        /// <summary>
        /// Get service of type <typeparamref name="T"/> from the <see cref="IServiceProvider"/>.
        /// </summary>
        /// <typeparam name="T">The type of service object to get.</typeparam>
        /// <param name="context">The <see cref="IServiceProvider"/> to retrieve the service object from.</param>
        /// <returns>A service object of type <typeparamref name="T"/>.</returns>
        /// <exception cref="InvalidOperationException">There is no service of type <typeparamref name="T"/>.</exception>
        public static T GetService<T>(this ActionExecutingContext context)
        {
            return context.HttpContext.RequestServices.GetRequiredService<T>();
        }

        /// <summary>
        /// Convert the statement to HTML.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="statement">The statement.</param>
        public static string BuildHtml(this IStatementWriter writer, Statement statement)
        {
            var sb = new StringBuilder();
            writer.BuildHtml(sb, statement);
            return sb.ToString();
        }
    }
}
