using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SatelliteSite.PolygonModule.Models
{
    public class JudgingRunModel
    {
        static Dictionary<string, Verdict> Mapping { get; }

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

        static JudgingRunModel()
        {
            Mapping = new Dictionary<string, Verdict>
            {
                { "correct", Verdict.Accepted },
                { "no-output", Verdict.WrongAnswer },
                { "wrong-answer", Verdict.WrongAnswer },
                { "timelimit", Verdict.TimeLimitExceeded },
                { "memory-limit", Verdict.MemoryLimitExceeded },
                { "output-limit", Verdict.OutputLimitExceeded },
                { "compare-error", Verdict.UndefinedError },
                { "run-error", Verdict.RuntimeError },
            };
        }

        public (JudgingRun, string, string) ParseInfo(int jid, DateTimeOffset time2)
        {
            int time, mem, exitcode, tcid;
            if (!double.TryParse(RunTime, out var dtime)) dtime = 0;
            time = (int)(dtime * 1000);
            if (!Mapping.TryGetValue(RunResult, out var verdict))
                verdict = Verdict.UndefinedError;
            if (!int.TryParse(TestcaseId, out tcid)) tcid = 0;

            try
            {
                var outsys = Encoding.UTF8.GetString(Convert.FromBase64String(OutputSystem));
                var st = Regex.Match(outsys, @"memory used: (\S+) bytes");
                if (!(st.Success && int.TryParse(st.Groups[1].Value, out mem))) mem = 0;
                mem /= 1024;
                var st2 = Regex.Match(outsys, @"Non-zero exitcode (\S+)");
                if (!(st2.Success && int.TryParse(st2.Groups[1].Value, out exitcode))) exitcode = 0;
            }
            catch
            {
                mem = exitcode = 0;
            }

            var r = new JudgingRun
            {
                Status = verdict,
                ExecuteMemory = mem,
                ExecuteTime = time,
                TestcaseId = tcid,
                OutputDiff = OutputDiff,
                OutputSystem = OutputSystem,
                JudgingId = jid,
                CompleteTime = time2,
                MetaData = MetaData,
            };

            return (r, OutputRun, OutputError);
        }
    }
}
