using MediatR;
using Polygon.Entities;
using SatelliteSite.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Polygon.Judgement
{
    public partial class DOMjudgeLikeHandlers : IRequestHandler<JudgehostRegisterRequest, List<UnfinishedJudging>>
    {
        public async Task<List<UnfinishedJudging>> Handle(JudgehostRegisterRequest request, CancellationToken cancellationToken)
        {
            var hostname = request.HostName;
            var item = await Facade.Judgehosts.FindAsync(hostname);

            if (item == null)
            {
                item = await Facade.Judgehosts.CreateAsync(
                    new Judgehost
                    {
                        ServerName = hostname,
                        PollTime = DateTimeOffset.Now,
                        Active = true,
                    });

                await Auditlogger.LogAsync(
                    type: AuditlogType.Judgehost,
                    userName: request.UserName,
                    now: item.PollTime!.Value,
                    action: "registered",
                    target: hostname,
                    extra: $"on {request.Ip}",
                    cid: null);

                Telemetry.TrackDependency(
                    dependencyTypeName: "JudgeHost",
                    dependencyName: item.ServerName,
                    data: "registed",
                    startTime: item.PollTime!.Value,
                    duration: TimeSpan.Zero,
                    success: true);

                return new List<UnfinishedJudging>();
            }
            else
            {
                await Facade.Judgehosts.NotifyPollAsync(item);
                var stat = new List<UnfinishedJudging>();

                var oldJudgings = await Facade.Judgings.ListAsync(
                    predicate: j => j.Server == item.ServerName && j.Status == Verdict.Running,
                    selector: j => new { j, j.s.ContestId, j.s.TeamId, j.s.ProblemId, j.s.Time },
                    topCount: 10000);

                foreach (var sg in oldJudgings)
                {
                    var rtq = new ReturnToQueueRequest(sg.j, sg.ContestId, sg.ProblemId, sg.TeamId, sg.Time);
                    await Handle(rtq, cancellationToken);
                    stat.Add(rtq.ToModel());
                }

                return stat;
            }
        }
    }
}
