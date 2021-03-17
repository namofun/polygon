using MediatR;
using Polygon.Entities;

namespace Polygon.Events
{
    public class RejudgingAppliedEvent : INotification
    {
        public Rejudging Rejudging { get; }

        public RejudgingAppliedEvent(Rejudging rejudging)
        {
            Rejudging = rejudging;
        }
    }
}
