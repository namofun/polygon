using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SatelliteSite.PolygonModule.Models
{
    public class UserInfo
    {
        [JsonPropertyName("roles")]
        public IList<string> Roles { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string UserName { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("lastip")]
        public string LastIp { get; set; }
    }
}
