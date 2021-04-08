using MediatR;
using Polygon.Entities;
using System;

namespace Polygon.Events
{
    public class JudgingFinishedEvent : INotification
    {
        public Judging Judging { get; }

        public int? ContestId { get; }

        public int ProblemId { get; }

        public int TeamId { get; }

        public DateTimeOffset SubmitTime { get; }

        public JudgingFinishedEvent(Judging j, int? cid, int probid, int uid, DateTimeOffset subtime)
        {
            if (cid == 0) cid = null;
            Judging = j;
            ContestId = cid;
            ProblemId = probid;
            TeamId = uid;
            SubmitTime = subtime;
        }
    }
}
