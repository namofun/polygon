using MediatR;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon.Events
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
