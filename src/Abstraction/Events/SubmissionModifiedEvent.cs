using MediatR;
using Polygon.Entities;

namespace Polygon.Events
{
    public class SubmissionModifiedEvent : INotification
    {
        public Submission Submission { get; }

        public SubmissionModifiedEvent(Submission submission)
        {
            Submission = submission;
        }
    }
}
