namespace Polygon.Storages
{
    /// <summary>
    /// The options for polygon storage.
    /// </summary>
    public class PolygonStorageOption
    {
        /// <summary>
        /// The content directory for judging related
        /// </summary>
        public string JudgingDirectory { get; set; } = string.Empty;

        /// <summary>
        /// The problem directory for judging related
        /// </summary>
        public string ProblemDirectory { get; set; } = string.Empty;
    }
}
