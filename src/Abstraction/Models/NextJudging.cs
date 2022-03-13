using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Events;

namespace Xylab.Polygon.Judgement
{
    public class NextJudging
    {
        [JsonPropertyName("submitid")]
        public int SubmissionId { get; set; }

        [JsonPropertyName("cid")]
        public int ContestId { get; set; }

        [JsonPropertyName("teamid")]
        public int TeamId { get; set; }

        [JsonPropertyName("probid")]
        public int ProblemId { get; set; }

        [JsonPropertyName("langid")]
        public string LanguageId { get; set; }

        [JsonPropertyName("rejudgingid")]
        public int? RejudgingId { get; set; }

        [JsonPropertyName("entry_point")]
        public string? EntryPoint { get; set; }

        [JsonPropertyName("origsubmitid")]
        public int? OriginalSubmissionId { get; set; }

        [JsonPropertyName("maxruntime")]
        public double MaxRunTime { get; set; }

        [JsonPropertyName("memlimit")]
        public int MemoryLimit { get; set; }

        [JsonPropertyName("outputlimit")]
        public int OutputLimit { get; set; }

        [JsonPropertyName("run")]
        public string Run { get; set; }

        [JsonPropertyName("compare")]
        public string Compare { get; set; }

        [JsonPropertyName("compare_args")]
        public string CompareArguments { get; set; }

        [JsonPropertyName("compile_script")]
        public string Compile { get; set; }

        [JsonPropertyName("combined_run_compare")]
        public bool CombinedRunCompare { get; set; }

        [JsonPropertyName("compare_md5sum")]
        public string CompareMd5sum { get; set; }

        [JsonPropertyName("run_md5sum")]
        public string RunMd5sum { get; set; }

        [JsonPropertyName("compile_script_md5sum")]
        public string CompileMd5sum { get; set; }

        [JsonPropertyName("judgingid")]
        public int JudgingId { get; set; }

        [JsonPropertyName("full_judge")]
        public bool SendOutputBack { get; set; }

        [JsonPropertyName("testcases")]
        public Dictionary<string, TestcaseToJudge> Testcases { get; set; }

#pragma warning disable CS8618
        public NextJudging()
        {
        }
#pragma warning restore CS8618

        public NextJudging(JudgingBeginEvent r, Dictionary<string, string> md5s, List<Testcase> testcases)
        {
            SubmissionId = r.Judging.SubmissionId;
            JudgingId = r.Judging.Id;
            ContestId = r.ContestId;
            TeamId = r.TeamId;
            ProblemId = r.Problem.Id;
            LanguageId = r.Language.Id;
            RejudgingId = r.RejudgingId;
            EntryPoint = null;
            OriginalSubmissionId = null;

            MaxRunTime = r.Problem.TimeLimit * r.Language.TimeFactor / 1000.0; // as seconds
            MemoryLimit = r.Problem.MemoryLimit + (r.Language.Id == "java" ? 131072 : 0); // as kb, java more 128M
            OutputLimit = r.Problem.OutputLimit; // KB

            Run = r.Problem.RunScript;
            Compare = r.Problem.CompareScript;
            CompareArguments = r.Problem.CompareArguments ?? string.Empty;
            CombinedRunCompare = r.Problem.CombinedRunCompare;
            Compile = r.Language.CompileScript;
            CompareMd5sum = md5s[r.Problem.CompareScript];
            RunMd5sum = md5s[r.Problem.RunScript];
            CompileMd5sum = md5s[r.Language.CompileScript];

            Testcases = testcases.ToDictionary(t => $"{t.Rank}", t => new TestcaseToJudge(t));

            // Sends back when:
            //   - the judging is required a full-test
            //   - the contest id is 0, means that this is a polygon submission
            //   - the rejudging id is not null, means that this is a rejudging submission
            //
            // Note that this is not accurate when using SYSTEM TEST feature in codeforces rules.
            // The field name isn't changed due to historical reasons.
            SendOutputBack = r.Judging.FullTest && (r.ContestId == 0 || r.Judging.RejudgingId != null);
        }
    }
}
