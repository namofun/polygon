using MediatR;
using Polygon.Entities;
using System;
using System.Collections.Generic;

namespace Polygon.Judgement
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
