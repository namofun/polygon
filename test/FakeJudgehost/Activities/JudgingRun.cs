#nullable disable
using System.Text.Json.Serialization;

namespace Xylab.Polygon.Judgement.Daemon.Fake
{
    /// <summary>
    /// The judging run to send to domserver.
    /// </summary>
    public class JudgingRun
    {
        /// <summary>
        /// The ID of the testcase of the run to add
        /// </summary>
        [JsonPropertyName("testcaseid")]
        public string TestcaseId { get; set; }

        /// <summary>
        /// The result of the run
        /// </summary>
        [JsonPropertyName("runresult")]
        public string RunResult { get; set; }

        /// <summary>
        /// The runtime of the run
        /// </summary>
        [JsonPropertyName("runtime")]
        public string RunTime { get; set; }

        /// <summary>
        /// The (base64-encoded) output of the run
        /// </summary>
        [JsonPropertyName("output_run")]
        public string OutputRun { get; set; }

        /// <summary>
        /// The (base64-encoded) output diff of the run
        /// </summary>
        [JsonPropertyName("output_diff")]
        public string OutputDiff { get; set; }

        /// <summary>
        /// The (base64-encoded) error output of the run
        /// </summary>
        [JsonPropertyName("output_error")]
        public string OutputError { get; set; }

        /// <summary>
        /// The (base64-encoded) system output of the run
        /// </summary>
        [JsonPropertyName("output_system")]
        public string OutputSystem { get; set; }

        /// <summary>
        /// The (base64-encoded) metadata
        /// </summary>
        [JsonPropertyName("metadata")]
        public string MetaData { get; set; }

        /// <summary>
        /// Map the verdict to DOMjudge-like strings.
        /// </summary>
        /// <param name="value">The verdict value.</param>
        /// <returns>The mapped string.</returns>
        public static string Map(Entities.Verdict value)
        {
            return value switch
            {
                Entities.Verdict.TimeLimitExceeded => "timelimit",
                Entities.Verdict.MemoryLimitExceeded => "memory-limit",
                Entities.Verdict.RuntimeError => "run-error",
                Entities.Verdict.OutputLimitExceeded => "output-limit",
                Entities.Verdict.WrongAnswer => "wrong-answer",
                Entities.Verdict.CompileError => "compiler-error",
                Entities.Verdict.Accepted => "correct",
                _ => "compare-error",
            };
        }
    }
}
