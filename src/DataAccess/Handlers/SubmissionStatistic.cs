using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Events;

namespace Xylab.Polygon.Storages.Handlers
{
    /// <summary>
    /// Handler for statistics submission
    /// </summary>
    public class SubmissionStatistic :
        INotificationHandler<JudgingFinishedEvent>
    {
        ISubmissionStore Store { get; }

        public SubmissionStatistic(ISubmissionStore store)
        {
            Store = store;
        }

        public Task Handle(JudgingFinishedEvent notification, CancellationToken cancellationToken)
        {
            if (!notification.Judging.Active) return Task.CompletedTask;
            return Store.UpdateStatisticsAsync(
                notification.ContestId ?? 0,
                notification.TeamId,
                notification.ProblemId,
                notification.Judging.Status == Verdict.Accepted);
        }
    }
}
