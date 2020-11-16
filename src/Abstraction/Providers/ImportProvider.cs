using Polygon.Entities;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Polygon.Packaging
{
    /// <summary>
    /// The provider interface for importing problems into system.
    /// </summary>
    public interface IImportProvider
    {
        /// <summary>
        /// The buffer to write log to
        /// </summary>
        StringBuilder LogBuffer { get; }

        /// <summary>
        /// Import the problems from the stream.
        /// </summary>
        /// <param name="stream">The stream of content file.</param>
        /// <param name="streamFileName">The file name for default settings.</param>
        /// <param name="username">The username of import user.</param>
        /// <returns>A <see cref="Task"/> representing the import asynchronous action.</returns>
        Task<List<Problem>> ImportAsync(Stream stream, string streamFileName, string username);
    }
}
