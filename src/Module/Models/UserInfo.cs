using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SatelliteSite.PolygonModule.Models
{
    /// <summary>
    /// The information for current login user.
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// The user roles
        /// </summary>
        [JsonPropertyName("roles")]
        public IList<string> Roles { get; set; }

        /// <summary>
        /// The user ID
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// The login name
        /// </summary>
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        /// <summary>
        /// The nick name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// The email address
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; }

        /// <summary>
        /// The current login IP
        /// </summary>
        [JsonPropertyName("lastip")]
        public string LastIp { get; set; }
    }
}
