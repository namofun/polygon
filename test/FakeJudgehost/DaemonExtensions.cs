using Polygon.Judgement;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace Polygon.FakeJudgehost
{
    /// <summary>
    /// Extension methods for judgedaemon.
    /// </summary>
    public static class DaemonExtensions
    {
        /// <summary>
        /// Disable the judgehost.
        /// </summary>
        /// <param name="judgeDaemon">The judge daemon.</param>
        /// <param name="kind">The kind of things to disable.</param>
        /// <param name="idColumn">The column name of things to disable.</param>
        /// <param name="id">The column value of things to disable.</param>
        /// <param name="description">The internal error title.</param>
        /// <param name="judgingId">The processing judging ID.</param>
        /// <param name="cid">The contest ID.</param>
        /// <param name="extra_log">The extra logs to append.</param>
        /// <returns>The task for internal error ID.</returns>
        public static async Task<int> Disable(
            this JudgeDaemon judgeDaemon,
            string kind, string idColumn, object id,
            string description,
            int? judgingId = null, int? cid = null,
            string? extra_log = null)
        {
            string judgehostLog = extra_log ?? "";
            var body = new Dictionary<string, string>
            {
                ["description"] = description,
                ["judgehostlog"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(judgehostLog)),
                ["disabled"] = $"{{\"kind\":\"{kind}\",\"{idColumn}\":{id.ToJson()}}}",
            };

            if (judgingId.HasValue)
            {
                body["judgingid"] = judgingId.Value.ToString();
                body["cid"] = (cid ?? 0).ToString();
            }

            using var form = await judgeDaemon.HttpClient.PostAsync("judgehosts/internal-error", new FormUrlEncodedContent(body));
            var content = await form.Content.ReadAsStringAsync();
            return int.Parse(content);
        }

        /// <summary>
        /// Send all unsent judging runs.
        /// </summary>
        /// <param name="judgeDaemon">The judge daemon.</param>
        /// <param name="unsent_judging_runs">The unsent judging runs.</param>
        /// <param name="judgingid">The judging ID.</param>
        /// <returns>The </returns>
        public static Task<HttpResponseMessage> SendUnsentJudgingRuns(
            this JudgeDaemon judgeDaemon,
            Queue<JudgingRun> unsent_judging_runs,
            int judgingid)
        {
            var url = $"judgehosts/add-judging-run/{UrlEncoder.Default.Encode(judgeDaemon.HostName)}/{judgingid}";
            var batch = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("batch", unsent_judging_runs.ToArray().ToJson()) });
            unsent_judging_runs.Clear();
            return judgeDaemon.HttpClient.PostAsync(url, batch);
        }

        /// <summary>
        /// Get the next judging.
        /// </summary>
        /// <param name="judgeDaemon">The judge daemon.</param>
        /// <returns>The task for fetching next judging, or <c>null</c> if nothing got.</returns>
        public static async Task<NextJudging?> NextJudging(
            this JudgeDaemon judgeDaemon)
        {
            try
            {
                var url = $"judgehosts/next-judging/{UrlEncoder.Default.Encode(judgeDaemon.HostName)}";
                using var request = await judgeDaemon.HttpClient.PostAsync(url, new StringContent(""));
                if (request.Content.Headers.ContentLength == 2) return null;
                using var stream = await request.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<NextJudging>(stream);
            }
            catch (NotSupportedException ex)
            {
                throw new ApplicationException("NextJudging error", ex);
            }
        }

        /// <summary>
        /// Get the configure value.
        /// </summary>
        /// <typeparam name="T">The configure value type.</typeparam>
        /// <param name="judgeDaemon">The judge daemon.</param>
        /// <param name="name">The config name.</param>
        /// <returns>The task for fetching configuration value.</returns>
        public static async Task<T> DbConfigGet<T>(
            this JudgeDaemon judgeDaemon, string name)
        {
            try
            {
                var values = await judgeDaemon.HttpClient.GetStringAsync($"config?name={UrlEncoder.Default.Encode(name)}");
                using var jsonDoc = JsonDocument.Parse(values);
                var element = jsonDoc.RootElement.GetProperty(name);
                return element.GetRawText().AsJson<T>();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("DbConfigGet error", ex);
            }
        }

        /// <summary>
        /// Update the judging information.
        /// </summary>
        /// <param name="judgeDaemon">The judge daemon.</param>
        /// <param name="judgingid">The judging id.</param>
        /// <param name="compile_success">Whether compile succeeded.</param>
        /// <param name="output_compile">The compiler output.</param>
        /// <returns>The task for updating compiling information.</returns>
        public static async Task UpdateJudging(
            this JudgeDaemon judgeDaemon, int judgingid, bool compile_success, string output_compile)
        {
            var url = $"judgehosts/update-judging/{UrlEncoder.Default.Encode(judgeDaemon.HostName)}/{judgingid}";
            var args = new Dictionary<string, string>
            {
                ["compile_success"] = compile_success ? "1" : "0",
                ["output_compile"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(output_compile ?? ""))
            };

            using var msg = await judgeDaemon.HttpClient.PutAsync(url, new FormUrlEncodedContent(args));
        }
    }
}
