using System;

namespace Polygon.Entities
{
    /// <summary>
    /// The status enum for internal errors.
    /// </summary>
    public enum InternalErrorStatus
    {
        /// <summary>
        /// The internal error is not resolved
        /// </summary>
        Open,

        /// <summary>
        /// The internal error is resolved
        /// </summary>
        Resolved,

        /// <summary>
        /// The internal error is ignored
        /// </summary>
        Ignored
    }

    /// <summary>
    /// The entity class for internal errors.
    /// </summary>
    public class InternalError
    {
        /// <summary>
        /// The internal error ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The contest ID
        /// </summary>
        public int? ContestId { get; set; }

        /// <summary>
        /// The occurring judging ID
        /// </summary>
        public int? JudgingId { get; set; }

        /// <summary>
        /// The description
        /// </summary>
        /// <remarks>This field is base64 encoded.</remarks>
        public string Description { get; set; }

        /// <summary>
        /// The judgehost log
        /// </summary>
        /// <remarks>This field is base64 encoded.</remarks>
        public string JudgehostLog { get; set; }

        /// <summary>
        /// The occurred time
        /// </summary>
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// The disabled object
        /// </summary>
        /// <remarks>This field is a json object of (string? kind,string? langid, string? hostname, int? probid).</remarks>
        public string Disabled { get; set; }

        /// <summary>
        /// The error status
        /// </summary>
        public InternalErrorStatus Status { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Construct an empty internal error for querying from database.
        /// </summary>
        public InternalError()
        {
        }
#pragma warning restore CS8618

        /// <summary>
        /// Construct a summary internal error for querying from database.
        /// </summary>
        public InternalError(int id, InternalErrorStatus status, DateTimeOffset time, string desc)
        {
            Id = id;
            Status = status;
            Time = time;
            Description = desc;
            JudgehostLog = string.Empty;
            Disabled = "{}";
        }
    }
}
