using Polygon.Entities;
using System;
using System.Collections.Generic;

namespace Polygon.Models
{
    /// <summary>
    /// The model class for solutions.
    /// </summary>
    public class Solution
    {
        /// <summary>
        /// The submission ID
        /// </summary>
        public int SubmissionId { get; set; }

        /// <summary>
        /// The judging ID
        /// </summary>
        public int JudgingId { get; set; }

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
        /// The total score
        /// </summary>
        public int? TotalScore { get; set; }

        /// <summary>
        /// The submission time
        /// </summary>
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// The length of code
        /// </summary>
        public int CodeLength { get; set; }

        /// <summary>
        /// The language ID
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// The execution time
        /// </summary>
        public int? ExecutionTime { get; set; }

        /// <summary>
        /// The execution memory
        /// </summary>
        public int? ExecutionMemory { get; set; }

        /// <summary>
        /// The expected verdict
        /// </summary>
        public Verdict? ExpectedVerdict { get; set; }

        /// <summary>
        /// The result verdict
        /// </summary>
        public Verdict Verdict { get; set; }

        /// <summary>
        /// The judging run verdicts
        /// </summary>
        /// <remarks>Ordered by the emitted ID. When not included, this field may be null.</remarks>
        public IEnumerable<Verdict>? Verdicts { get; set; }

        /// <summary>
        /// The name of author
        /// </summary>
        public string AuthorName { get; set; }

        /// <summary>
        /// The submission IP
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Whether this solution is skipped
        /// </summary>
        public bool Skipped { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Construct an empty solution for querying from database.
        /// </summary>
        public Solution()
        {
        }
#pragma warning restore CS8618
    }
}
