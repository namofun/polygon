namespace Polygon.Entities
{
    /// <summary>
    /// The entity class for relationship between <see cref="Problem"/> and those authorized user.
    /// </summary>
    public class ProblemAuthor
    {
        /// <summary>
        /// The user ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The problem ID
        /// </summary>
        public int ProblemId { get; set; }

        /// <summary>
        /// The author level
        /// </summary>
        public AuthorLevel Level { get; set; }
    }

    /// <summary>
    /// The enum for denoting the level of <see cref="ProblemAuthor"/>.
    /// </summary>
    public enum AuthorLevel
    {
        /// <summary>
        /// Authors can only make submissions.
        /// </summary>
        Reader,

        /// <summary>
        /// Besides readonly features, authors can do operations on testcases and descriptions.
        /// </summary>
        Writer,

        /// <summary>
        /// Besides write features, authors can add another author into the problem.
        /// </summary>
        Creator,
    }
}
