using Polygon.Entities;
using System;

namespace Polygon.Models
{
    /// <summary>
    /// The model class for difference in the rejudging.
    /// </summary>
    public class RejudgingDifference
    {
        /// <summary>
        /// The old judging
        /// </summary>
        public Judging OldJudging { get; }

        /// <summary>
        /// The new judging
        /// </summary>
        public Judging NewJudging { get; }

        /// <summary>
        /// The problem ID
        /// </summary>
        public int ProblemId { get; }

        /// <summary>
        /// The language ID
        /// </summary>
        public string Language { get; }

        /// <summary>
        /// The submit time
        /// </summary>
        public DateTimeOffset SubmitTime { get; }

        /// <summary>
        /// The team ID
        /// </summary>
        public int TeamId { get; }

        /// <summary>
        /// The contest ID
        /// </summary>
        public int ContestId { get; }

        /// <summary>
        /// The contest ID
        /// </summary>
        public int SubmissionId { get; }

        /// <summary>
        /// Construct a model with data.
        /// </summary>
        public RejudgingDifference(Judging old, Judging @new, int probid, string lang, DateTimeOffset time, int teamid, int cid)
        {
            OldJudging = old;
            NewJudging = @new;
            ProblemId = probid;
            Language = lang;
            SubmitTime = time;
            TeamId = teamid;
            ContestId = cid;
            SubmissionId = old.SubmissionId;
        }
    }
}
