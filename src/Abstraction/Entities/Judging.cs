using System;
using System.Collections.Generic;

namespace Polygon.Entities
{
    /// <summary>
    /// The entity class for judgings.
    /// </summary>
    public class Judging
    {
        /// <summary>
        /// The judging ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The submission ID
        /// </summary>
        public int SubmissionId { get; set; }

        /// <summary>
        /// Whether this judging is valid active result
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Whether to take a full test
        /// </summary>
        public bool FullTest { get; set; }

        /// <summary>
        /// The start time of this judging
        /// </summary>
        public DateTimeOffset? StartTime { get; set; }

        /// <summary>
        /// The end time of this judging
        /// </summary>
        public DateTimeOffset? StopTime { get; set; }

        /// <summary>
        /// The judgehost that take this judging
        /// </summary>
        public string? Server { get; set; }

        /// <summary>
        /// The judging verdict
        /// </summary>
        public Verdict Status { get; set; }

        /// <summary>
        /// The execution time
        /// </summary>
        /// <remarks>The unit of time limit is <c>ms</c>.</remarks>
        public int? ExecuteTime { get; set; }

        /// <summary>
        /// The execution memory
        /// </summary>
        /// <remarks>The unit of memory limit is <c>kb</c>.</remarks>
        public int? ExecuteMemory { get; set; }

        /// <summary>
        /// The output of compiler
        /// </summary>
        /// <remarks>This field is base64 encoded.</remarks>
        public string? CompileError { get; set; }

        /// <summary>
        /// The rejudging ID
        /// </summary>
        public int? RejudgingId { get; set; }

        /// <summary>
        /// The previous judging ID in rejudging
        /// </summary>
        /// <remarks>When <see cref="RejudgingId"/> is <c>null</c>, this should also be <c>null</c>.</remarks>
        public int? PreviousJudgingId { get; set; }

        /// <summary>
        /// The total score for judging
        /// </summary>
        public int? TotalScore { get; set; }

        /// <summary>
        /// The navigation to judging runs
        /// </summary>
        public ICollection<JudgingRun> Details { get; set; }

        /// <summary>
        /// The navigation to submission
        /// </summary>
        public Submission s { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Construct an empty judging for querying from database.
        /// </summary>
        public Judging()
        {
        }
#pragma warning restore CS8618
    }
}
