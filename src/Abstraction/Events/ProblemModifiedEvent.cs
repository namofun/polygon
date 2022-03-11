using MediatR;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon.Events
{
    public class ProblemModifiedEvent : INotification
    {
        public Problem Problem { get; }

        public ProblemModifiedEvent(Problem problem)
        {
            Problem = problem;
        }
    }
}
