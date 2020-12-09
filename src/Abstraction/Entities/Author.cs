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
    }
}
