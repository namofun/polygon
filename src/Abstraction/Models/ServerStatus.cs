using System.Text.Json.Serialization;

namespace Polygon.Models
{
    public class ServerStatus
    {
        [JsonPropertyName("cid")]
        public int ContestId { get; set; }

        [JsonPropertyName("num_submissions")]
        public int Total { get; set; }

        [JsonPropertyName("num_queued")]
        public int Queued { get; set; }

        [JsonPropertyName("num_judging")]
        public int Judging { get; set; }
    }
}
