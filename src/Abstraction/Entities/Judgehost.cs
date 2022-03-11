using System;

namespace Xylab.Polygon.Entities
{
    /// <summary>
    /// The entity class for judgehosts.
    /// </summary>
    public class Judgehost
    {
        /// <summary>
        /// The server name
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Whether to enable this judgehost
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// The last polling time
        /// </summary>
        public DateTimeOffset? PollTime { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Construct an empty judgehost for querying from database.
        /// </summary>
        public Judgehost()
        {
        }
#pragma warning restore CS8618

        /// <summary>
        /// Construct an empty judgehost for inserting into database.
        /// </summary>
        public Judgehost(string name, bool active = true)
        {
            ServerName = name;
            Active = active;
        }
    }
}
