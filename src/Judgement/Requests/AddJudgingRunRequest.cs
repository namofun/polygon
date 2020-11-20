using MediatR;
using System.Collections.Generic;

namespace Polygon.Judgement
{
    public class AddJudgingRunRequest : IRequest<bool>
    {
        public string HostName { get; }

        public int JudgingId { get; }

        public List<JudgingRunModel> Batch { get; }

        public AddJudgingRunRequest(string hostname, int jid, List<JudgingRunModel> batch)
        {
            HostName = hostname;
            JudgingId = jid;
            Batch = batch;
        }
    }
}
