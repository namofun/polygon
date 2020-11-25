namespace Polygon.Models
{
    /// <summary>
    /// The model class for solution authors.
    /// </summary>
    public class SolutionAuthor
    {
        /// <summary>
        /// The submission ID
        /// </summary>
        public int SubmissionId { get; }

        /// <summary>
        /// The contest ID
        /// </summary>
        public int ContestId { get; }

        /// <summary>
        /// The team ID
        /// </summary>
        public int TeamId { get; }

        /// <summary>
        /// The user name
        /// </summary>
        public string? UserName { get; }

        /// <summary>
        /// The team name
        /// </summary>
        public string? TeamName { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return ContestId == 0
                ? $"{UserName ?? "SYSTEM"} (u{TeamId})"
                : $"{TeamName ?? "CONTEST"} (c{ContestId}t{TeamId})";
        }

        /// <summary>
        /// Create an instance of solution author.
        /// </summary>
        /// <param name="sid">The submission ID.</param>
        /// <param name="cid">The contest ID.</param>
        /// <param name="tid">The team ID.</param>
        /// <param name="userName">The user name.</param>
        /// <param name="teamName">The team name.</param>
        public SolutionAuthor(int sid, int cid, int tid, string? userName, string? teamName)
        {
            SubmissionId = sid;
            ContestId = cid;
            TeamId = tid;
            UserName = userName;
            TeamName = teamName;
        }
    }
}
