using MediatR;
using Polygon.Events;
using SatelliteSite.Entities;
using SatelliteSite.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Polygon.Storages.Handlers
{
    /// <summary>
    /// Handlers that works for auditlogging.
    /// </summary>
    public class Auditlogging :
        INotificationHandler<SubmissionCreatedEvent>
    {
        public IAuditlogger Auditlogger { get; }

        public Auditlogging(IAuditlogger auditlogger)
        {
            Auditlogger = auditlogger;
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
                cid: s.ContestId == 0 ? default(int?) : s.ContestId);
        }
    }
}
