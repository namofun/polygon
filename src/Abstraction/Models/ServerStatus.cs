using System.Text.Json.Serialization;

namespace Xylab.Polygon.Models
{
    /// <summary>
    /// The model class for server statistics.
    /// </summary>
    public class ServerStatus
    {
        /// <summary>
        /// The contest ID
        /// </summary>
        [JsonPropertyName("cid")]
        public int ContestId { get; set; }

        /// <summary>
        /// The total submission count
        /// </summary>
        [JsonPropertyName("num_submissions")]
        public int Total { get; set; }

        /// <summary>
        /// The pending submission count
        /// </summary>
        [JsonPropertyName("num_queued")]
        public int Queued { get; set; }

        /// <summary>
        /// The running submission count
        /// </summary>
        [JsonPropertyName("num_judging")]
        public int Judging { get; set; }
    }
}
