using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Polygon.FakeJudgehost
{
    public class FakeJudgeActivity : IDaemonStrategy
    {
        public Dictionary<string, (string, byte[])> Files { get; }
            = new Dictionary<string, (string, byte[])>();

        private async Task<byte[]> FetchFile(JudgeDaemon service, string requestUrl, string md5)
        {
            if (Files.TryGetValue(requestUrl, out var exec) && md5 == exec.Item1)
                return exec.Item2;

            var result = await service.HttpClient.GetStringAsync(requestUrl);
            result = result.Trim('"');
            var bytes = Convert.FromBase64String(result);
            var md5_new = bytes.ToMD5().ToHexDigest(true);

            if (md5 != md5_new)
                throw new ApplicationException("File corrupted during download.");

            Files.Add(requestUrl, (md5_new, bytes));
            return bytes;
        }

        private async Task<string> FetchSubmission(JudgeDaemon service, int cid, int sid)
        {
            using var resp = await service.HttpClient.GetAsync($"contests/{cid}/submissions/{sid}/source-code");
            using var stream = await resp.Content.ReadAsStreamAsync();
            var entity = await JsonSerializer.DeserializeAsync<Models.SubmissionFile[]>(stream);
            return entity[0].SourceCode.UnBase64();
        }

        private static string Exec(string execid) => "executables/" + UrlEncoder.Default.Encode(execid);

        private static Entities.Verdict Map(char ch)
        {
            return ch switch
            {
                'A' => Entities.Verdict.Accepted,
                'C' => Entities.Verdict.CompileError,
                'M' => Entities.Verdict.MemoryLimitExceeded,
                'O' => Entities.Verdict.OutputLimitExceeded,
                'R' => Entities.Verdict.RuntimeError,
                'T' => Entities.Verdict.TimeLimitExceeded,
                'W' => Entities.Verdict.WrongAnswer,
                _ => Entities.Verdict.UndefinedError,
            };
        }

        private async Task Judge(JudgeDaemon service, Judgement.NextJudging row, CancellationToken stoppingToken)
        {
            service.Logger.LogInformation(
                "Judging submission s{submitid} (t{teamid}/p{probid}/{langid}), id j{judgingid}...",
                row.SubmissionId, row.TeamId, row.ProblemId, row.LanguageId, row.JudgingId);

            if (row.LanguageId != "fake")
                await service.Disable("language", "langid", row.LanguageId, $"Language {row.LanguageId} is not supported by fake judgehost.", row.JudgingId, row.ContestId, "hmmmmmm...");

            await service.DbConfigGet<int>("script_timelimit");
            await service.DbConfigGet<int>("script_memory_limit");
            await service.DbConfigGet<int>("script_filesize_limit");
            await service.DbConfigGet<int>("process_limit");
            await service.DbConfigGet<int>("output_storage_limit");

            try
            {
                await FetchFile(service, Exec(row.Compile), row.CompileMd5sum);
            }
            catch (ApplicationException)
            {
                await service.Disable(
                    "language", "langid", row.LanguageId,
                    row.Compile + ": fetch, compile, or deploy of compile script failed.");
            }

            var submission = await FetchSubmission(service, row.ContestId, row.SubmissionId);
            await service.UpdateJudging(row.JudgingId, submission == "C", "ok");
            if (submission == "C") return;

            await service.DbConfigGet<string>("dbconfig_get_rest");
            var update_every_X_seconds = TimeSpan.FromSeconds(await service.DbConfigGet<int>("update_judging_seconds"));

            int totalcases = 0;
            bool lastcase_correct = true;
            var last_sent = DateTimeOffset.Now;
            int tl = (int)(row.MaxRunTime * 1000) + 1000;
            var random = new Random();
            var unsent_judging_runs = new Queue<JudgingRun>();

            async Task<string> RunTestcase(Judgement.TestcaseToJudge ttj)
            {
                service.Logger.LogInformation("Running testcase {rank}...", ttj.Rank);
                var ch = ttj.Rank - 1 > submission!.Length ? 'A' : submission[ttj.Rank - 1];
                var verd = Map(ch);
                var verd2 = JudgingRun.Map(verd);
                int runtime = verd == Entities.Verdict.TimeLimitExceeded
                    ? random!.Next(tl - 1000, tl + 1)
                    : random!.Next(0, tl - 1000 + 1);

                try
                {
                    await FetchFile(service, Exec(row.Run), row.RunMd5sum);
                    if (!row.CombinedRunCompare)
                        await FetchFile(service, Exec(row.Compare), row.CompareMd5sum);
                }
                catch (ApplicationException)
                {
                    await service.Disable(
                        "problem", "probid", row.ProblemId,
                        $"{row.Compare} or {row.Run}: fetch, compile, or deploy of compile script failed.");
                    return "compare-error";
                }

                await Task.Delay(runtime);

                var jr = new JudgingRun
                {
                    TestcaseId = ttj.TestcaseId.ToString(),
                    RunResult = JudgingRun.Map(verd),
                    OutputRun = row.FullJudge ? $"output run for j{row.JudgingId}t{ttj.TestcaseId}".ToBase64() : null,
                    OutputError = row.FullJudge ? $"output error for j{row.JudgingId}t{ttj.TestcaseId}".ToBase64() : null,
                    OutputDiff = $"output diff for j{row.JudgingId}t{ttj.TestcaseId}".ToBase64(),
                    OutputSystem = $"output system for j{row.JudgingId}t{ttj.TestcaseId}".ToBase64(),
                    MetaData = $"output metadata for j{row.JudgingId}t{ttj.TestcaseId}".ToBase64(),
                    RunTime = $"{runtime / 1000.0}",
                };

                unsent_judging_runs!.Enqueue(jr);
                totalcases++;
                lastcase_correct &= verd == Entities.Verdict.Accepted;
                return verd2;
            }

            var ttjs = new Queue<Judgement.TestcaseToJudge>();
            foreach (var tc in row.Testcases.Values.OrderBy(t => t.Rank))
                ttjs.Enqueue(tc);

            while (ttjs.Count > 0)
            {
                var tc = ttjs.Dequeue();
                var result = await RunTestcase(tc);
                if (!lastcase_correct || (DateTimeOffset.Now - last_sent) > update_every_X_seconds)
                {
                    using var t = await service.SendUnsentJudgingRuns(unsent_judging_runs, row.JudgingId);
                    if (!t.IsSuccessStatusCode)
                    {
                        await service.Disable(
                            "problem", "probid", row.ProblemId,
                            "uploading unsent judging runs failed");
                        return;
                    }

                    last_sent = DateTimeOffset.Now;
                }

                service.Logger.LogInformation("Testcase {rank} done, result: {result}", tc.Rank, result);

                if (!lastcase_correct)
                {
                    ttjs.Clear();
                    var url = $"testcases/next-to-judge/{row.JudgingId}";
                    using var msg = await service.HttpClient.GetAsync(url);
                    if (msg.Content.Headers.ContentLength > 2)
                    {
                        using var stream = await msg.Content.ReadAsStreamAsync();
                        var ttj = await JsonSerializer.DeserializeAsync<Judgement.TestcaseToJudge>(stream);
                        if (ttj != null) ttjs.Enqueue(ttj);
                    }
                }

                if (result == "compare-error")
                    break;
            }

            if (unsent_judging_runs.Count > 0)
            {
                using var t = await service.SendUnsentJudgingRuns(unsent_judging_runs, row.JudgingId);
                if (!t.IsSuccessStatusCode)
                {
                    await service.Disable(
                        "problem", "probid", row.ProblemId,
                        "uploading unsent judging runs failed");
                    return;
                }
            }

            service.Logger.LogInformation("Judging s{submitid}/j{judgingid} finished", row.SubmissionId, row.JudgingId);
        }

        public async Task ExecuteAsync(JudgeDaemon service, CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var row = await service.NextJudging();

                if (row == null)
                {
                    await Task.Delay(5000, stoppingToken);
                }
                else
                {
                    await Judge(service, row, stoppingToken);
                }
            }
        }
    }
}
