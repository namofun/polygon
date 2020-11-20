using System.Text.Json.Serialization;

namespace Polygon.Judgement
{
    /// <summary>
    /// The information for unfinished judgings.
    /// </summary>
    public class UnfinishedJudging
    {
        /// <summary>
        /// The judging ID
        /// </summary>
        [JsonPropertyName("judgingid")]
        public int JudgingId { get; set; }

        /// <summary>
        /// The submission ID
        /// </summary>
        [JsonPropertyName("submitid")]
        public int SubmissionId { get; set; }

        /// <summary>
        /// The contest ID
        /// </summary>
        [JsonPropertyName("cid")]
        public int ContestId { get; set; }
    }
}
