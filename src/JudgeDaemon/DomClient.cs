using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Xylab.Polygon.Judgement.Daemon
{
    public class DomClient
    {
        public HttpClient HttpClient { get; }

        public DomClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public async Task<int> FireInternalErrorAsync(
            string description,
            string judgehostlog,
            DisableTarget disableTarget)
        {
            HttpContent content = new FormUrlEncodedContent(
                new KeyValuePair<string, string>[]
                {
                    new(nameof(description), description),
                    new(nameof(judgehostlog), judgehostlog.ToBase64()),
                    new("disabled", disableTarget.ToString()),
                });

            using HttpResponseMessage r = await HttpClient.PostAsync("judgehosts/internal-error", content);
            return await r.Content.ReadFromJsonAsync<int>();
        }

        public async Task<NextJudging> FetchNextJudgingAsync(string hostname)
        {

        }
    }
}
