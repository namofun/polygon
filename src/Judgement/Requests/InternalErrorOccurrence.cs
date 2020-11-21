using MediatR;
using Polygon.Entities;
using Polygon.Models;

namespace Polygon.Judgement
{
    public class InternalErrorOccurrence : IRequest<(InternalError, InternalErrorDisable)>
    {
        public string Description { get; }

        public string JudgehostLog { get; }

        public string Disabled { get; }

        public int? ContestId { get; }

        public int? JudgingId { get; }

        public InternalErrorOccurrence(string desc, string log, string disabled, int? cid, int? jid)
        {
            Description = desc;
            JudgehostLog = log;
            Disabled = disabled;
            ContestId = cid;
            JudgingId = jid;
        }
    }
}
