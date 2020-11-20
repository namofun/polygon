using Polygon.Entities;
using System.Text.Json.Serialization;

namespace SatelliteSite.PolygonModule.Models
{
    /// <summary>
    /// The judgehost information.
    /// </summary>
    public class JudgehostInfo
    {
        /// <summary>
        /// The host name of judgehost
        /// </summary>
        [JsonPropertyName("hostname")]
        public string HostName { get; set; }

        /// <summary>
        /// The activity of judgehost
        /// </summary>
        [JsonPropertyName("active")]
        public bool Active { get; set; }

        /// <summary>
        /// The last poll time of judgehost
        /// </summary>
        [JsonPropertyName("polltime")]
        public long? PollTime { get; set; }

        /// <summary>
        /// The formatted last poll time of judgehost
        /// </summary>
        [JsonPropertyName("polltime_formatted")]
        public string PollTimeFormatted { get; set; }

        /// <summary>
        /// Construct the judgehost information via entity.
        /// </summary>
        /// <param name="a">The judgehost entity.</param>
        public JudgehostInfo(Judgehost a)
        {
            HostName = a.ServerName;
            Active = a.Active;
            PollTime = a.PollTime?.ToUnixTimeSeconds();
            PollTimeFormatted = a.PollTime?.ToString("yyyy-MM-dd HH:mm:ss zzz");
        }
    }
}
