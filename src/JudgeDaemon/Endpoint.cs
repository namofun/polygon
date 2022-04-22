#nullable disable
using System;

namespace Xylab.Polygon.Judgement.Daemon
{
    public class Endpoint
    {
        public string Url { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool Waiting { get; set; }

        public bool Errorred { get; set; }

        public DateTimeOffset? LastAttempt { get; set; }

        public DomClient Client { get; set; }
    }
}
