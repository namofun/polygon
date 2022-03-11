using System.IO;

namespace Xylab.Polygon.Packaging
{
    /// <summary>
    /// The model class for package export result.
    /// </summary>
    public class ExportResult
    {
        /// <summary>
        /// The stream to read the export result
        /// </summary>
        /// <remarks>Should be disposed by consumer.</remarks>
        public Stream OpenStream { get; }

        /// <summary>
        /// The mime type of result
        /// </summary>
        public string MimeType { get; }

        /// <summary>
        /// The file name of result
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Constrcut an export result.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="mimeType">The mime type.</param>
        /// <param name="content">The content stream.</param>
        public ExportResult(string fileName, string mimeType, Stream content)
        {
            OpenStream = content;
            MimeType = mimeType;
            FileName = fileName;
        }
    }
}
