using System;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon.Models
{
    /// <summary>
    /// The interface class for judgements, which is aimed at reducing data transferring.
    /// </summary>
    public interface IJudgementDetail
    {
        /// <summary>
        /// The judging ID
        /// </summary>
        int Id { get; }

        /// <summary>
        /// The submission ID
        /// </summary>
        int SubmissionId { get; }

        /// <summary>
        /// Whether this judging is valid active result
        /// </summary>
        bool Active { get; }

        /// <summary>
        /// The start time of this judging
        /// </summary>
        DateTimeOffset? StartTime { get; }

        /// <summary>
        /// The end time of this judging
        /// </summary>
        DateTimeOffset? StopTime { get; }

        /// <summary>
        /// The judging verdict
        /// </summary>
        Verdict Status { get; }

        /// <summary>
        /// The execution time
        /// </summary>
        /// <remarks>The unit of time limit is <c>ms</c>.</remarks>
        int? ExecuteTime { get; }
    }
}
