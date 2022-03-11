using MediatR;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Models;

namespace Xylab.Polygon.Judgement
{
    public class InternalErrorOccurrence : IRequest<(InternalError, InternalErrorDisable)>
    {
        public string Description { get; }

        public string JudgehostLog { get; }

        public string Disabled { get; }

        public int? ContestId { get; }

        public int? JudgingId { get; }

        public InternalErrorOccurrence(string description, string log, string disabled, int? cid, int? judgingid)
        {
            Description = description;
            JudgehostLog = log;
            Disabled = disabled;
            ContestId = cid;
            JudgingId = judgingid;
        }
    }
}
