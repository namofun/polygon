using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Xylab.Polygon.Judgement.Daemon
{
    public sealed class PolygonClient : IAsyncDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly UrlEncoder _urlEncoder;
        private readonly Endpoint _endpoint;

        public PolygonClient(HttpClient httpClient, UrlEncoder urlEncoder, Endpoint endpoint)
        {
            _httpClient = httpClient;
            _urlEncoder = urlEncoder;
            _endpoint = endpoint;
        }

        public static PolygonClient Create(IHttpClientFactory httpClientFactory, Endpoint endpoint)
        {
            HttpClient client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new("Basic", $"{endpoint.UserName}:{endpoint.Password}".ToBase64());
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PolygonClient", typeof(PolygonClient).Assembly.GetName().Version!.ToString()));
            client.BaseAddress = new Uri(endpoint.Url);
            return new PolygonClient(client, UrlEncoder.Default, endpoint);
        }

        public async Task<UnfinishedJudging[]?> RegisterJudgehost(string hostname)
        {
            using var resp = await _httpClient.PostAsync(
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

            using var resp = await _httpClient.PostAsync("judgehosts/internal-error", content);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<int>();
        }

        public async Task<byte[]?> GetExecutable(string target)
        {
            using var resp = await _httpClient.GetAsync("executables/" + _urlEncoder.Encode(target));
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
            using var resp = await _httpClient.PostAsync(
                "judgehosts/next-judging/" + _urlEncoder.Encode(hostname),
                new ByteArrayContent(Array.Empty<byte>()));

            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<NextJudging>();
        }

        public T GetConfiguration<T>(string configurationName, T defaultValue = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            _httpClient.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
