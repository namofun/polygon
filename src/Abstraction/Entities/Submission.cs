using System;
using System.Collections.Generic;

namespace Polygon.Entities
{
    /// <summary>
    /// The entity class for submissions.
    /// </summary>
    public class Submission
    {
        /// <summary>
        /// The submission ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The submission time
        /// </summary>
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// The contest ID
        /// </summary>
        public int ContestId { get; set; }

        /// <summary>
        /// The participant ID
        /// </summary>
        public int TeamId { get; set; }

        /// <summary>
        /// The problem ID
        /// </summary>
        public int ProblemId { get; set; }

        /// <summary>
        /// The source code
        /// </summary>
        /// <remarks>This field is base64 encoded. Maximum source size is 192KB.</remarks>
        public string SourceCode { get; set; }

        /// <summary>
        /// The length of source code
        /// </summary>
        public int CodeLength { get; set; }

        /// <summary>
        /// The language ID
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// The source IP Address of submission
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// The expected result for polygon submissions
        /// </summary>
        public Verdict? ExpectedResult { get; set; }

        /// <summary>
        /// The rejudging ID
        /// </summary>
        public int? RejudgingId { get; set; }

        /// <summary>
        /// Whether submission is ignored
        /// </summary>
        public bool Ignored { get; set; }

        /// <summary>
        /// The navigation to judgings
        /// </summary>
        public ICollection<Judging> Judgings { get; set; }

        /// <summary>
        /// The navigation to problem
        /// </summary>
        public Problem p { get; set; }

        /// <summary>
        /// The navigation to language
        /// </summary>
        public Language l { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Construct an empty submission for querying from database.
        /// </summary>
        public Submission()
        {
        }
#pragma warning restore CS8618
    }
}
