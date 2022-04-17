using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xylab.Polygon.Judgement.Daemon
{
    public class DisableTarget
    {
        [JsonPropertyName("kind")]
        public string Kind { get; set; } = string.Empty;

        [JsonExtensionData]
        public IDictionary<string, string> ExtensionData { get; set; } = new Dictionary<string, string>();

        private static DisableTarget Of(string kind, string targetKey, string targetValue)
            => new() { Kind = kind, ExtensionData = { [targetKey] = targetValue } };

        public static DisableTarget Judgehost(string hostname)
            => Of("judgehost", "hostname", hostname);
    }
}
