using System.IO.Compression;
using System.Text;
using Xylab.Polygon.Models;

namespace Xylab.Polygon.Packaging
{
    /// <summary>
    /// The statement writer interface, which provide features to convert the statement to HTML or TeX files.
    /// </summary>
    public interface IStatementWriter
    {
        /// <summary>
        /// Convert the statement to HTML.
        /// </summary>
        /// <param name="stringBuilder">The convert destination.</param>
        /// <param name="statement">The statement.</param>
        void BuildHtml(StringBuilder stringBuilder, Statement statement);

        /// <summary>
        /// Convert the statement to TeX source.
        /// </summary>
        /// <param name="zip">The zip archive to append to.</param>
        /// <param name="statement">The statement.</param>
        /// <param name="filePrefix">The file path prefix.</param>
        void BuildLatex(ZipArchive zip, Statement statement, string filePrefix = "");
    }
}
