using MediatR;
using Xylab.Polygon.Judgement;

namespace Xylab.Polygon.Events
{
    public class JudgingPrepublishEvent : INotification
    {
        public NextJudging NextJudging { get; }

        public JudgingPrepublishEvent(NextJudging nextJudging)
        {
            NextJudging = nextJudging;
        }
    }
}
