using MediatR;
using Microsoft.Extensions.Logging;
using Polygon.Entities;
using Polygon.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polygon.Judgement
{
    public partial class DOMjudgeLikeHandlers : IRequestHandler<AddJudgingRunRequest, bool>
    {
        public async Task<bool> Handle(AddJudgingRunRequest request, CancellationToken cancellationToken)
        {
            var host = await Facade.Judgehosts.FindAsync(request.HostName);
            if (host is null) return false; // Unknown or inactive judgehost requested
            await Facade.Judgehosts.NotifyPollAsync(host);

            var js = await Facade.Judgings.FindAsync(
                predicate: j => j.Id == request.JudgingId,
                selector: j => new { j, j.s.ProblemId, j.s.ContestId, j.s.TeamId, j.s.Time });
            if (js == null)
                throw new InvalidOperationException("Unknown judging occurred.");

            var (judging, pid, cid, uid, time) = (js.j, js.ProblemId, js.ContestId, js.TeamId, js.Time);

            foreach (var (run, output_run, output_error) in request.Batch(request.JudgingId, host.PollTime!.Value))
            {
                var detail = await Facade.Judgings.InsertAsync(run);

                if (output_error != null || output_run != null)
                {
                    try
                    {
                        var stderr = Convert.FromBase64String(output_error ?? string.Empty);
                        var stdout = Convert.FromBase64String(output_run ?? string.Empty);
                        await Facade.Judgings.SetRunFileAsync(judging.Id, detail.Id, "out", stdout);
                        await Facade.Judgings.SetRunFileAsync(judging.Id, detail.Id, "err", stderr);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "An error occurred when saving OutputError and OutputRun for j{judgingId}, r{runId}", judging.Id, detail.Id);
                    }
                }

                await Mediator.Publish(new JudgingRunEmitted(detail, judging, cid, pid, uid, time));
            }

            // Check for the final status
            var countTc = await Facade.Testcases.CountAsync(pid);
            var verdict = await Facade.Judgings.SummarizeAsync(judging.Id);
            // testId for score, testcaseId for count of tested cases

            bool anyRejected = !judging.FullTest && verdict.Status != Verdict.Accepted;
            bool fullTested = verdict.TestcaseId >= countTc && countTc > 0;

            if (anyRejected || fullTested)
            {
                judging.ExecuteMemory = verdict.ExecuteMemory;
                judging.ExecuteTime = verdict.ExecuteTime;
                judging.Status = verdict.Status;
                judging.StopTime = host.PollTime;
                judging.TotalScore = verdict.Id;
                await FinalizeJudging(new JudgingFinishedEvent(judging, cid, pid, uid, time));
            }

            return true;
        }
    }
}
