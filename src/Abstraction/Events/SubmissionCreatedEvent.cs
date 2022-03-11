using MediatR;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon.Events
{
    /// <summary>
    /// The event for post submission creation.
    /// </summary>
    public class SubmissionCreatedEvent : INotification
    {
        /// <summary>
        /// The created submission
        /// </summary>
        public Submission Submission { get; }

        /// <summary>
        /// The operation source
        /// </summary>
        public string Via { get; }

        /// <summary>
        /// The operator user name
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// Construct the event for submission created.
        /// </summary>
        /// <param name="submission">The submission.</param>
        /// <param name="via">The source.</param>
        /// <param name="userName">The user.</param>
        public SubmissionCreatedEvent(Submission submission, string via, string userName)
        {
            Submission = submission;
            Via = via;
            UserName = userName;
        }
    }
}
