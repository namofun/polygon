namespace SatelliteSite.PolygonModule.Models
{
    public class MarkdownModel
    {
        /// <summary>
        /// Markdown content
        /// </summary>
        public string Markdown { get; set; }
        
        /// <summary>
        /// The backing store ID
        /// </summary>
        public string BackingStore { get; set; }

        /// <summary>
        /// The target content
        /// </summary>
        public string Target { get; set; }
    }
}
