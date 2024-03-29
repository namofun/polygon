﻿using System.IO;
using System.Text.Json.Serialization;

namespace Xylab.Polygon.Judgement
{
    public class UnfinishedJudging
    {
        [JsonPropertyName("judgingid")]
        public int JudgingId { get; set; }

        [JsonPropertyName("submitid")]
        public int SubmissionId { get; set; }

        [JsonPropertyName("cid")]
        public int ContestId { get; set; }

        public string GetJudgehostPath(string workdirPath)
        {
            return Path.Combine(workdirPath, $"{ContestId}/{SubmissionId}/{JudgingId}");
        }
    }
}
