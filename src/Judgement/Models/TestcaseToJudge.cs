using Polygon.Entities;
using System.Text.Json.Serialization;

namespace Polygon.Judgement
{
    /// <summary>
    /// The testcase to judge.
    /// </summary>
    public class TestcaseToJudge
    {
        [JsonPropertyName("testcaseid")]
        public int TestcaseId { get; set; }

        [JsonPropertyName("md5sum_input")]
        public string Md5sumInput { get; set; }

        [JsonPropertyName("md5sum_output")]
        public string Md5sumOutput { get; set; }

        [JsonPropertyName("probid")]
        public int ProblemId { get; set; }

        [JsonPropertyName("rank")]
        public int Rank { get; set; }

        [JsonPropertyName("description_as_string")]
        public string Description { get; set; }

        public TestcaseToJudge(Testcase t)
        {
            TestcaseId = t.Id;
            ProblemId = t.ProblemId;
            Md5sumInput = t.Md5sumInput;
            Md5sumOutput = t.Md5sumOutput;
            Rank = t.Rank;
            Description = t.Description;
        }
    }
}
