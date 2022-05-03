#nullable disable
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Xylab.Polygon.Judgement.Daemon
{
    public sealed class Endpoint : IAsyncDisposable
    {
        public HttpClient HttpClient { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool Waiting { get; set; }

        public bool Errorred { get; set; }

        public DateTimeOffset? LastAttempt { get; set; }

        public ValueTask DisposeAsync()
        {
            HttpClient?.Dispose();
            HttpClient = null;
            return ValueTask.CompletedTask;
        }
    }
}
