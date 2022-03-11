using System;
using System.Collections;
using System.Collections.Generic;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon.Models
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
        public virtual IEnumerable<Verdict>? RunVerdicts { get; set; }

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

    /// <summary>
    /// The model class for solutions with verdicts.
    /// </summary>
    internal class SolutionV1 : Solution, IEnumerable<Verdict>
    {
        /// <inheritdoc />
        public override IEnumerable<Verdict>? RunVerdicts
        {
            get => this;
            set => throw new InvalidOperationException("The run verdicts has been set.");
        }

        /// <inheritdoc cref="RunVerdicts" />
        public string? RunVerdictsRaw { get; set; }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IEnumerator<Verdict> GetEnumerator()
        {
            if (RunVerdictsRaw != null)
            {
                for (int i = 0; i < RunVerdictsRaw.Length; i++)
                {
                    yield return ResourceDictionary.ConvertToVerdict(RunVerdictsRaw[i]);
                }
            }
        }
    }
}
