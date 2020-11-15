using System.Text.Json.Serialization;

namespace Polygon.Models
{
    /// <summary>
    /// The model class for submission file.
    /// </summary>
    public class SubmissionFile
    {
        /// <summary>
        /// The submission file ID
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// The submission ID
        /// </summary>
        [JsonPropertyName("submission_id")]
        public string SubmissionId { get; set; }
        
        /// <summary>
        /// The submission file name
        /// </summary>
        [JsonPropertyName("filename")]
        public string FileName { get; set; }

        /// <summary>
        /// The submission source code
        /// </summary>
        /// <remarks>This field is base64 encoded.</remarks>
        [JsonPropertyName("source")]
        public string SourceCode { get; set; }

        /// <summary>
        /// Construct a model with data.
        /// </summary>
        /// <param name="submissionId">The submission ID.</param>
        /// <param name="sourceBase64">The source code with base64.</param>
        /// <param name="fileExt">The file ext without leading dot.</param>
        public SubmissionFile(int submissionId, string sourceBase64, string fileExt)
        {
            SourceCode = sourceBase64;
            FileName = "Main." + fileExt;
            Id = SubmissionId = submissionId.ToString();
        }

#pragma warning disable CS8618
        /// <summary>
        /// Construct a model for deserializing.
        /// </summary>
        public SubmissionFile()
        {
        }
#pragma warning restore CS8618
    }
}
