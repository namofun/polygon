using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Xylab.Polygon.Models;

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

        private Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content)
        {
            ArgumentNullException.ThrowIfNull(_httpClient, "Endpoint hasn't been initialized.");
            return _httpClient.PutAsync(requestUri, content);
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
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            resp.EnsureSuccessStatusCode();
            string? base64encoded = await resp.Content.ReadFromJsonAsync<string>();
            if (base64encoded == null)
            {
                return null;
            }

            return Convert.FromBase64String(base64encoded);
        }

        public async Task<SubmissionFile[]> GetSourceCode(int cid, int submitid)
        {
            using var resp = await GetAsync($"contests/{cid}/submissions/{submitid}/source-code");
            resp.EnsureSuccessStatusCode();
            return (await resp.Content.ReadFromJsonAsync<SubmissionFile[]>())!;
        }

        public async Task<NextJudging?> FetchNextJudging(string hostname)
        {
            using var resp = await PostAsync(
                "judgehosts/next-judging/" + UrlEncoder.Default.Encode(hostname),
                new ByteArrayContent(Array.Empty<byte>()));

            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<NextJudging>();
        }

        public async Task UpdateJudging(string hostname, int judgingId, int compile_success, string output_compile)
        {
            using var resp = await PutAsync(
                "judgehosts/update-judging/" + UrlEncoder.Default.Encode(hostname) + "/" + judgingId,
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { nameof(compile_success), compile_success.ToString() },
                    { nameof(output_compile), output_compile.ToBase64() },
                }));

            resp.EnsureSuccessStatusCode();
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
