using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Xylab.Polygon.Judgement.Daemon
{
    public static class EndpointExtensions
    {
        public static async Task<UnfinishedJudging[]?> RegisterJudgehost(this Endpoint endpoint, string hostname)
        {
            using var resp = await endpoint.HttpClient.PostAsync(
                "judgehosts",
                new FormUrlEncodedContent(new[]
                {
                    KeyValuePair.Create(nameof(hostname), hostname),
                }));

            return resp.IsSuccessStatusCode
                ? await resp.Content.ReadFromJsonAsync<UnfinishedJudging[]>()
                : null;
        }

        public static async Task<int> FireInternalError(
            this Endpoint endpoint,
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

            using var resp = await endpoint.HttpClient.PostAsync("judgehosts/internal-error", content);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<int>();
        }

        public static async Task<byte[]?> GetExecutable(this Endpoint endpoint, string target)
        {
            using var resp = await endpoint.HttpClient.GetAsync("executables/" + UrlEncoder.Default.Encode(target));
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

        public static async Task<NextJudging?> FetchNextJudging(this Endpoint endpoint, string hostname)
        {
            using var resp = await endpoint.HttpClient.PostAsync(
                "judgehosts/next-judging/" + UrlEncoder.Default.Encode(hostname),
                new ByteArrayContent(Array.Empty<byte>()));

            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<NextJudging>();
        }

        public static T GetConfiguration<T>(this Endpoint endpoint, string configurationName, T defaultValue = default)
        {
            throw new NotImplementedException();
        }
    }
}
