using Polygon.Entities;

namespace Polygon.Models
{
    /// <summary>
    /// The model class for judging run summary.
    /// </summary>
    public class RunSummary
    {
        /// <summary>
        /// The judging ID
        /// </summary>
        public int JudgingId { get; set; }

        /// <summary>
        /// The final verdict
        /// </summary>
        public Verdict FinalVerdict { get; set; }

        /// <summary>
        /// The encountered testcases
        /// </summary>
        public int Testcases { get; set; }

        /// <summary>
        /// The highest execution memory
        /// </summary>
        public int HighestMemory { get; set; }

        /// <summary>
        /// The longest running time
        /// </summary>
        public int LongestTime { get; set; }

        /// <summary>
        /// The total score
        /// </summary>
        public int TotalScore { get; set; }
    }
}
