using MediatR;
using Polygon.Entities;

namespace Polygon.Events
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
