using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Xylab.Polygon.Judgement.Daemon.Fake
{
    /// <summary>
    /// Options to configure the judgedaemon.
    /// </summary>
    public class DaemonOptions
    {
        /// <summary>
        /// Gets or sets the username of judgehost account.
        /// </summary>
        public string UserName { get; set; } = "judgehost";

        /// <summary>
        /// Gets or sets the password of judgehost account.
        /// </summary>
        public string Password { get; set; } = "123456";

        /// <summary>
        /// Gets or sets the <see cref="HttpClient"/> factory.
        /// </summary>
        public Func<IServiceProvider, HttpClient> HttpClientFactory { get; set; } = _ => new HttpClient();

        /// <summary>
        /// Configure the <see cref="HttpClient"/> with username and password.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> to configure.</param>
        private void Configure(HttpClient client)
        {
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{UserName}:{Password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
        }

        /// <summary>
        /// Create the final client.
        /// </summary>
        /// <param name="services">The service provider.</param>
        /// <returns>The created and configured <see cref="HttpClient"/>.</returns>
        public HttpClient CreateConfiguredClient(IServiceProvider services)
        {
            var client = HttpClientFactory.Invoke(services);
            Configure(client);
            return client;
        }
    }
}
