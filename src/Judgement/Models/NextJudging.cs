using Polygon.Entities;
using Polygon.Events;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Polygon.Judgement
{
    public class NextJudging
    {
        [JsonPropertyName("submitid")]
        public int SubmissionId { get; }

        [JsonPropertyName("cid")]
        public int ContestId { get; }

        [JsonPropertyName("teamid")]
        public int TeamId { get; }

        [JsonPropertyName("probid")]
        public int ProblemId { get; }

        [JsonPropertyName("langid")]
        public string LanguageId { get; }

        [JsonPropertyName("rejudgingid")]
        public int? RejudgingId { get; }

        [JsonPropertyName("entry_point")]
        public string? EntryPoint { get; }

        [JsonPropertyName("origsubmitid")]
        public int? OriginalSubmissionId { get; }

        [JsonPropertyName("maxruntime")]
        public double MaxRunTime { get; }

        [JsonPropertyName("memlimit")]
        public int MemoryLimit { get; }

        [JsonPropertyName("outputlimit")]
        public int OutputLimit { get; }

        [JsonPropertyName("run")]
        public string Run { get; }

        [JsonPropertyName("compare")]
        public string Compare { get; }

        [JsonPropertyName("compare_args")]
        public string CompareArguments { get; }

        [JsonPropertyName("compile_script")]
        public string Compile { get; }

        [JsonPropertyName("combined_run_compare")]
        public bool CombinedRunCompare { get; }

        [JsonPropertyName("compare_md5sum")]
        public string CompareMd5sum { get; }

        [JsonPropertyName("run_md5sum")]
        public string RunMd5sum { get; }

        [JsonPropertyName("compile_script_md5sum")]
        public string CompileMd5sum { get; }

        [JsonPropertyName("judgingid")]
        public int JudgingId { get; }

        [JsonPropertyName("full_judge")]
        public bool FullJudge { get; }

        [JsonPropertyName("testcases")]
        public Dictionary<string, TestcaseToJudge> Testcases { get; }

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
            CompareArguments = r.Problem.ComapreArguments ?? string.Empty;
            CombinedRunCompare = r.Problem.CombinedRunCompare;
            Compile = r.Language.CompileScript;
            CompareMd5sum = md5s[r.Problem.CompareScript];
            RunMd5sum = md5s[r.Problem.RunScript];
            CompileMd5sum = md5s[r.Language.CompileScript];

            Testcases = testcases.ToDictionary(t => $"{t.Rank}", t => new TestcaseToJudge(t));

            // Hello what's this?
            FullJudge = r.Judging.FullTest && (r.ContestId == 0 || r.Judging.RejudgingId != null);
        }
    }
}
