using MediatR;
using Polygon.Entities;
using Polygon.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polygon.Judgement
{
    public partial class DOMjudgeLikeHandlers : IRequestHandler<NextJudgingRequest, NextJudging?>
    {
        static AsyncLock Lock { get; } = new AsyncLock();

        public async Task<NextJudging?> Handle(NextJudgingRequest request, CancellationToken cancellationToken)
        {
            var host = await Facade.Judgehosts.FindAsync(request.HostName);
            if (host is null) return null;
            await Facade.Judgehosts.NotifyPollAsync(host);
            if (!host.Active) return null;
            // Above: unknown or inactive judgehost requested

            JudgingBeginEvent? r;

            using (await Lock.LockAsync())
            {
                r = await Facade.Judgings.FindAsync(
                    selector: j => new JudgingBeginEvent(j, j.s.p, j.s.l, j.s.ContestId, j.s.TeamId, j.s.RejudgingId),
                    predicate: j => j.Status == Verdict.Pending
                                 && j.s.l.AllowJudge
                                 && j.s.p.AllowJudge
                                 && !j.s.Ignored);

                if (r == null) return null;

                var judging = r.Judging;
                judging.Status = Verdict.Running;
                judging.Server = host.ServerName;
                judging.StartTime = DateTimeOffset.Now;

                await Facade.Judgings.UpdateAsync(
                    id: judging.Id,
                    j => new Judging
                    {
                        Status = Verdict.Running,
                        Server = judging.Server,
                        StartTime = judging.StartTime,
                    });
            }

            await Mediator.Publish(r, CancellationToken.None);

            var md5s = await Facade.Executables.ListMd5Async(
                r.Problem.RunScript,
                r.Problem.CompareScript,
                r.Language.CompileScript);

            var tcss = await Facade.Testcases.ListAsync(r.Problem.Id);

            return new NextJudging(r, md5s, tcss);
        }
    }
}
