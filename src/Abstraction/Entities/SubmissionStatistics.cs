namespace Xylab.Polygon.Entities
{
    /// <summary>
    /// The entity class for submission statistics.
    /// </summary>
    public class SubmissionStatistics
    {
        /// <summary>
        /// The problem ID
        /// </summary>
        public int ProblemId { get; set; }

        /// <summary>
        /// The contest ID
        /// </summary>
        public int ContestId { get; set; }

        /// <summary>
        /// The team ID
        /// </summary>
        public int TeamId { get; set; }

        /// <summary>
        /// The count of total submissions
        /// </summary>
        public int TotalSubmission { get; set; }

        /// <summary>
        /// The count of accepted submissions
        /// </summary>
        public int AcceptedSubmission { get; set; }
    }
}
