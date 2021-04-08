using MediatR;
using Polygon.Judgement;

namespace Polygon.Events
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
