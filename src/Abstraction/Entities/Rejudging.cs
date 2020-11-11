using System;

namespace Polygon.Entities
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
        public int ContestId { get; set; }

        /// <summary>
        /// The reason for rejudging
        /// </summary>
        public string Reason { get; set; }

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
        /// [Ignored] The user who issued this rejudging
        /// </summary>
        /// <remarks>The username of issuer.</remarks>
        public string? Issuer { get; set; }

        /// <summary>
        /// The user who operated this rejudging
        /// </summary>
        public int? OperatedBy { get; set; }

        /// <summary>
        /// [Ignored] The user who operated this rejudging
        /// </summary>
        /// <remarks>The username of operator.</remarks>
        public string? Operator { get; set; }

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

#pragma warning disable CS8618
        /// <summary>
        /// Construct an empty rejudging for querying from database.
        /// </summary>
        public Rejudging()
        {
        }
#pragma warning restore CS8618

        /// <summary>
        /// Construct a summary rejudging for querying from database.
        /// </summary>
        public Rejudging(Rejudging r1, string u1, string u2)
        {
            Applied = r1.Applied;
            Id = r1.Id;
            ContestId = r1.ContestId;
            IssuedBy = r1.IssuedBy;
            Issuer = u1;
            OperatedBy = r1.OperatedBy;
            Operator = u2;
            StartTime = r1.StartTime;
            EndTime = r1.EndTime;
            Reason = r1.Reason;
        }
    }
}
