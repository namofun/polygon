using MediatR;
using Polygon.Events;
using SatelliteSite;
using SatelliteSite.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polygon.Storages.Handlers
{
    /// <summary>
    /// Handlers that works for auditlogging.
    /// </summary>
    public class Auditlogging :
        INotificationHandler<SubmissionCreatedEvent>,
        INotificationHandler<JudgingFinishedEvent>
    {
        public IAuditlogger Auditlogger { get; }

        public Auditlogging(IAuditlogger auditlogger)
        {
            Auditlogger = auditlogger;
        }

        private int? UnifyContestId(int? cid)
        {
            return cid == 0 ? default : cid;
        }

        public Task Handle(SubmissionCreatedEvent notification, CancellationToken cancellationToken)
        {
            var s = notification.Submission;

            return Auditlogger.LogAsync(
                type: AuditlogType.Submission,
                userName: notification.UserName,
                now: s.Time,
                action: "added",
                target: $"{s.Id}",
                extra: $"via {notification.Via}",
                cid: UnifyContestId(s.ContestId));
        }

        public Task Handle(JudgingFinishedEvent notification, CancellationToken cancellationToken)
        {
            var j = notification.Judging;

            return Auditlogger.LogAsync(
                type: AuditlogType.Judging,
                userName: j.Server ?? "unknown-judgehost",
                now: DateTimeOffset.Now,
                action: "judged",
                target: $"{j.Id}",
                extra: $"{j.Status}",
                cid: UnifyContestId(notification.ContestId));
        }
    }
}
