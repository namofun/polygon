using Polygon.Storages;

namespace Polygon
{
    /// <summary>
    /// The physical storage path of polygon files.
    /// </summary>
    public class PolygonPhysicalOptions
    {
        /// <summary>
        /// The content directory for judging related
        /// </summary>
        public string JudgingDirectory { get; set; } = string.Empty;

        /// <summary>
        /// The problem directory for judging related
        /// </summary>
        public string ProblemDirectory { get; set; } = string.Empty;

        /// <summary>
        /// The judging file provider
        /// </summary>
        public IJudgingFileProvider? JudgingFileProvider { get; set; }

        /// <summary>
        /// The problem file provider
        /// </summary>
        public IProblemFileProvider? ProblemFileProvider { get; set; }
    }
}
