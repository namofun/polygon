﻿using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Events;
using Xylab.Polygon.Storages;

namespace Xylab.Polygon.Judgement.Requests
{
    public partial class DOMjudgeLikeHandlers : IRequestHandler<AddJudgingRunRequest, bool>
    {
        public async Task<bool> Handle(AddJudgingRunRequest request, CancellationToken cancellationToken)
        {
            var host = await Facade.Judgehosts.FindAsync(request.HostName);
            if (host is null) return false; // Unknown or inactive judgehost requested
            await Facade.Judgehosts.NotifyPollAsync(host);

            var (judging, probid, cid, uid, time) = await Facade.Judgings.FindAsync(request.JudgingId);
            if (judging == null) throw new InvalidOperationException("Unknown judging occurred.");
            var runList = new List<JudgingRun>();

            foreach (var (run, output_run, output_error) in request.Batch(request.JudgingId, host.PollTime!.Value))
            {
                var detail = await Facade.Judgings.InsertAsync(run);
                runList.Add(detail);

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
            }

            // Check for the final status
            var addition = new string(runList.Select(r => ResourceDictionary.ConvertToChar(r.Status)).ToArray());
            var countTc = await Facade.Testcases.CountAsync(probid);
            var verdict = await Facade.Judgings.SummarizeAsync(judging.Id);

            await Mediator.Publish(new JudgingRunEmittedEvent(runList, judging, cid, probid, uid, time, verdict.Testcases - runList.Count + 1));

            bool anyRejected = !judging.FullTest && verdict.FinalVerdict != Verdict.Accepted;
            bool fullTested = verdict.Testcases >= countTc && countTc > 0;

            if (anyRejected || fullTested)
            {
                judging.ExecuteMemory = verdict.HighestMemory;
                judging.ExecuteTime = verdict.LongestTime;
                judging.Status = verdict.FinalVerdict;
                judging.StopTime = host.PollTime;
                judging.TotalScore = verdict.TotalScore;
                judging.RunVerdicts += addition;

                await Facade.Judgings.UpdateAsync(
                    id: judging.Id,
                    j => new Judging
                    {
                        ExecuteMemory = judging.ExecuteMemory,
                        ExecuteTime = judging.ExecuteTime,
                        Status = judging.Status,
                        StopTime = judging.StopTime,
                        TotalScore = judging.TotalScore,
                        RunVerdicts = judging.RunVerdicts,
                    });

                await FinalizeJudging(new JudgingFinishedEvent(judging, cid, probid, uid, time));
            }
            else
            {
                await Facade.Judgings.UpdateAsync(
                    id: judging.Id,
                    j => new Judging { RunVerdicts = j.RunVerdicts + addition });
            }

            return true;
        }
    }
}
