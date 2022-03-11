using System.Text.Json.Serialization;

namespace Xylab.Polygon.Models
{
    /// <summary>
    /// The model class for disabled objects in internal error.
    /// </summary>
    public class InternalErrorDisable
    {
        /// <summary>
        /// The kind of disable object
        /// </summary>
        [JsonPropertyName("kind")]
        public string Kind { get; set; }

        /// <summary>
        /// The affected language ID
        /// </summary>
        [JsonPropertyName("langid")]
        public string? Language { get; set; }

        /// <summary>
        /// The affected judgehost name
        /// </summary>
        [JsonPropertyName("hostname")]
        public string? HostName { get; set; }

        /// <summary>
        /// The affected problem ID
        /// </summary>
        [JsonPropertyName("probid")]
        public int? ProblemId { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Construct an empty disable object for deserializing.
        /// </summary>
        public InternalErrorDisable()
        {
        }
#pragma warning restore CS8618
    }
}
