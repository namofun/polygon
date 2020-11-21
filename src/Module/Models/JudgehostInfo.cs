using Polygon.Entities;
using System.Text.Json.Serialization;

namespace SatelliteSite.PolygonModule.Models
{
    public class JudgehostInfo
    {
        [JsonPropertyName("hostname")]
        public string HostName { get; set; }

        [JsonPropertyName("active")]
        public bool Active { get; set; }

        [JsonPropertyName("polltime")]
        public long? PollTime { get; set; }

        [JsonPropertyName("polltime_formatted")]
        public string PollTimeFormatted { get; set; }

        public JudgehostInfo(Judgehost a)
        {
            HostName = a.ServerName;
            Active = a.Active;
            PollTime = a.PollTime?.ToUnixTimeSeconds();
            PollTimeFormatted = a.PollTime?.ToString("yyyy-MM-dd HH:mm:ss zzz");
        }
    }
}
