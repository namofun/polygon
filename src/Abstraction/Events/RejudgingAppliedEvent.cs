using MediatR;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon.Events
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
