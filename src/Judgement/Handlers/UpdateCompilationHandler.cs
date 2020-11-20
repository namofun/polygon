using MediatR;
using Polygon.Entities;
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

            var js = await Facade.Judgings.FindAsync(
                predicate: j => j.Id == request.JudgingId,
                selector: j => new { j, j.s.ProblemId, j.s.ContestId, j.s.TeamId, j.s.Time });
            if (js is null) return null;

            js.j.CompileError = request.CompilerOutput ?? "";

            if (request.Success != 1)
            {
                js.j.Status = Verdict.CompileError;
                js.j.StopTime = DateTimeOffset.Now;
                await FinalizeJudging(new Events.JudgingFinishedEvent(js.j, js.ContestId, js.ProblemId, js.TeamId, js.Time));
            }
            else
            {
                await Facade.Judgings.UpdateAsync(js.j);
            }

            return js.j;
        }
    }
}
