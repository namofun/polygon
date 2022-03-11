using MediatR;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon.Events
{
    public class JudgingBeginEvent : INotification
    {
        public Judging Judging { get; set; }

        public Problem Problem { get; set; }

        public Language Language { get; set; }

        public int ContestId { get; set; }

        public int TeamId { get; set; }

        public int? RejudgingId { get; set; }

        public JudgingBeginEvent(Judging j, Problem p, Language l, int cid, int uid, int? rejid)
        {
            Judging = j;
            Problem = p;
            Language = l;
            ContestId = cid;
            TeamId = uid;
            RejudgingId = rejid;
        }
    }
}
