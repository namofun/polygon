using MediatR;
using Polygon.Entities;
using Polygon.Storages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polygon.Judgement
{
    public partial class DOMjudgeLikeHandlers : IRequestHandler<UpdateCompilationRequest, Judging?>
    {
        public async Task<Judging?> Handle(UpdateCompilationRequest request, CancellationToken cancellationToken)
        {
            var host = await Facade.Judgehosts.FindAsync(request.Judgehost);

            // Unknown or inactive judgehost requested
            if (host is null) return null;
            await Facade.Judgehosts.NotifyPollAsync(host);

            var (judging, problemId, contestId, teamId, time) = await Facade.Judgings.FindAsync(request.JudgingId);
            if (judging == null) return null;

            judging.CompileError = request.CompilerOutput ?? "";
            judging.RunVerdicts = "";

            if (request.Success != 1)
            {
                judging.Status = Verdict.CompileError;
                judging.StopTime = DateTimeOffset.Now;

                await Facade.Judgings.UpdateAsync(
                    id: judging.Id,
                    j => new Judging
                    {
                        CompileError = judging.CompileError,
                        StopTime = judging.StopTime,
                        Status = Verdict.CompileError,
                        RunVerdicts = "",
                    });

                await FinalizeJudging(new Events.JudgingFinishedEvent(judging, contestId, problemId, teamId, time));
            }
            else
            {
                await Facade.Judgings.UpdateAsync(
                    id: judging.Id,
                    j => new Judging
                    {
                        CompileError = judging.CompileError,
                        RunVerdicts = "",
                    });
            }

            return judging;
        }
    }
}
