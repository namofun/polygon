using System;

namespace Xylab.Polygon.Entities
{
    /// <summary>
    /// The entity class for rejudgings.
    /// </summary>
    public class Rejudging
    {
        /// <summary>
        /// The rejudging ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The contest ID
        /// </summary>
        public int? ContestId { get; set; }

        /// <summary>
        /// The reason for rejudging
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// The start time of rejudging
        /// </summary>
        public DateTimeOffset? StartTime { get; set; }

        /// <summary>
        /// The end time of rejudging
        /// </summary>
        public DateTimeOffset? EndTime { get; set; }

        /// <summary>
        /// The user who issued this rejudging
        /// </summary>
        public int? IssuedBy { get; set; }

        /// <summary>
        /// The user who operated this rejudging
        /// </summary>
        public int? OperatedBy { get; set; }

        /// <summary>
        /// Whether this rejudging is applied
        /// </summary>
        /// <remarks>If <c>null</c>, the rejudging is not finished. If <c>true</c>, the rejudging is applied and valid. Otherwise, invalid.</remarks>
        public bool? Applied { get; set; }

        /// <summary>
        /// [Ignore] The progress of readiness
        /// </summary>
        /// <remarks>Count / Total</remarks>
        public (int, int) Ready { get; set; }
    }
}
