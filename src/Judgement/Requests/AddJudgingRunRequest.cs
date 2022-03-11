using MediatR;
using System;
using System.Collections.Generic;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon.Judgement
{
    public class AddJudgingRunRequest : IRequest<bool>
    {
        public string HostName { get; }

        public int JudgingId { get; }

        public Func<int, DateTimeOffset, IEnumerable<(JudgingRun, string? Output, string? Error)>> Batch { get; }

        public AddJudgingRunRequest(string hostname, int judgingid,
            Func<int, DateTimeOffset, IEnumerable<(JudgingRun, string? Output, string? Error)>> batchParser)
        {
            HostName = hostname;
            JudgingId = judgingid;
            Batch = batchParser;
        }
    }
}
