using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Xylab.Polygon.Judgement.Daemon
{
    public class DomClient
    {
        public HttpClient HttpClient { get; }

        public UrlEncoder UrlEncoder { get; }

        public DomClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
            UrlEncoder = UrlEncoder.Default;
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

            using var resp = await HttpClient.PostAsync("judgehosts/internal-error", content);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<int>();
        }

        public async Task<byte[]?> GetExecutable(string target)
        {
            using var resp = await HttpClient.GetAsync("executables/" + UrlEncoder.Encode(target));
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
            using var resp = await HttpClient.PostAsync(
                "judgehosts/next-judging/" + UrlEncoder.Encode(hostname),
                new ByteArrayContent(Array.Empty<byte>()));

            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<NextJudging>();
        }
    }
}
