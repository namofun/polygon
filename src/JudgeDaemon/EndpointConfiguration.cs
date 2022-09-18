#nullable disable
using System.Text.Json.Serialization;

namespace Xylab.Polygon.Judgement.Daemon
{
    /// <summary>
    /// The class for configurations on endpoint.
    /// </summary>
    public class EndpointConfiguration
    {
        /// <summary>
        /// Maximum seconds available for compile/compare scripts.
        /// </summary>
        /// <remarks>
        /// This is a safeguard against malicious code and buggy scripts, so a reasonable but large amount should do.
        /// </remarks>
        [JsonPropertyName("script_timelimit")]
        public int ScriptTimeLimit { get; set; }

        /// <summary>
        /// Maximum memory usage (in kB) by compile/compare scripts.
        /// </summary>
        /// <remarks>
        /// This is a safeguard against malicious code and buggy script, so a reasonable but large amount should do.
        /// </remarks>
        [JsonPropertyName("script_memory_limit")]
        public int ScriptMemoryLimit { get; set; }

        /// <summary>
        /// Maximum filesize (in kB) compile/compare scripts may write.
        /// </summary>
        /// <remarks>
        /// Submission will fail with compiler-error when trying to write more,
        /// so this should be greater than any <b>intermediate or final</b> result written by compilers.
        /// </remarks>
        [JsonPropertyName("script_filesize_limit")]
        public int ScriptFileSizeLimit { get; set; }

        /// <summary>
        /// Maximum number of processes that the submission is allowed to start (including shell and possibly interpreters).
        /// </summary>
        [JsonPropertyName("process_limit")]
        public int ProcessLimit { get; set; }

        /// <summary>
        /// Maximum size of error/system output stored in the database (in bytes); use "-1" to disable any limits.
        /// </summary>
        [JsonPropertyName("output_storage_limit")]
        public long OutputStorageLimit { get; set; }

        /// <summary>
        /// Time that submissions are kept running beyond timelimit before being killed.
        /// </summary>
        /// <remarks>
        /// Specify as "Xs" for X seconds, "Y%" as percentage, or a combination of both separated by one of "+|&amp;" for the sum, maximum, or minimum of both.
        /// </remarks>
        [JsonPropertyName("timelimit_overshoot")]
        public string TimeLimitOvershoot { get; set; }

        /// <summary>
        /// Post updates to a judging every X seconds. Set to 0 to update after each judging run.
        /// </summary>
        [JsonPropertyName("update_judging_seconds")]
        public int UpdateJudgingSeconds { get; set; }

        /// <summary>
        /// Minimum free disk space (in kB) on judgehosts.
        /// </summary>
        [JsonPropertyName("diskspace_error")]
        public long DiskSpaceError { get; set; }
    }
}
