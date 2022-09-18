using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Xylab.Polygon.Judgement.Daemon
{
    public sealed class Endpoint : IAsyncDisposable
    {
        private HttpClient? _httpClient;
        private EndpointConfiguration? _endpointConfiguration;

        #region Endpoint Parameters

        public string Name { get; set; } = "default";

        public string Url { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool Waiting { get; set; }

        public bool Errorred { get; set; }

        public DateTimeOffset? LastAttempt { get; set; }

        #endregion

        public ValueTask DisposeAsync()
        {
            _httpClient?.Dispose();
            _httpClient = null;
            _endpointConfiguration = null;
            return ValueTask.CompletedTask;
        }

        public void Initialize(IHttpClientFactory httpClientFactory)
        {
            HttpClient client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new("Basic", $"{UserName}:{Password}".ToBase64());
            client.DefaultRequestHeaders.UserAgent.Add(new("PolygonClient", typeof(Endpoint).Assembly.GetName().Version!.ToString()));
            _httpClient = client;
            _endpointConfiguration = null;
        }

        private Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            ArgumentNullException.ThrowIfNull(_httpClient, "Endpoint hasn't been initialized.");
            return _httpClient.GetAsync(requestUri);
        }

        private Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            ArgumentNullException.ThrowIfNull(_httpClient, "Endpoint hasn't been initialized.");
            return _httpClient.PostAsync(requestUri, content);
        }

        public async Task<UnfinishedJudging[]?> RegisterJudgehost(string hostname)
        {
            using var resp = await PostAsync(
                "judgehosts",
                new FormUrlEncodedContent(new[]
                {
                    KeyValuePair.Create(nameof(hostname), hostname),
                }));

            return resp.IsSuccessStatusCode
                ? await resp.Content.ReadFromJsonAsync<UnfinishedJudging[]>()
                : null;
        }

        public async Task<int> FireInternalError(
            string description,
            string judgehostlog,
            DisableTarget disableTarget)
        {
            HttpContent content = new FormUrlEncodedContent(
                new KeyValuePair<string, string>[]
                {
                    new(nameof(description), description),
                    new(nameof(judgehostlog), judgehostlog.ToBase64()),
                    new("disabled", disableTarget.ToJson()),
                });

            using var resp = await PostAsync("judgehosts/internal-error", content);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<int>();
        }

        public async Task<byte[]?> GetExecutable(string target)
        {
            using var resp = await GetAsync("executables/" + UrlEncoder.Default.Encode(target));
            if (!resp.IsSuccessStatusCode)
            {
                return null;
            }

            string? base64encoded = await resp.Content.ReadFromJsonAsync<string>();
            if (base64encoded == null)
            {
                return null;
            }

            return Convert.FromBase64String(base64encoded);
        }

        public async Task<NextJudging?> FetchNextJudging(string hostname)
        {
            using var resp = await PostAsync(
                "judgehosts/next-judging/" + UrlEncoder.Default.Encode(hostname),
                new ByteArrayContent(Array.Empty<byte>()));

            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<NextJudging>();
        }

        public async Task RefreshConfiguration()
        {
            using var resp = await GetAsync("config");
            resp.EnsureSuccessStatusCode();

            _endpointConfiguration = await resp.Content.ReadFromJsonAsync<EndpointConfiguration>();
        }

        public async Task<T> GetConfiguration<T>(Func<EndpointConfiguration, T> configSelector)
        {
            if (_endpointConfiguration == null)
            {
                await RefreshConfiguration();
            }

            return configSelector(_endpointConfiguration!);
        }
    }
}
