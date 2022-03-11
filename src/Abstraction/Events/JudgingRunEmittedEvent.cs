using MediatR;
using System;
using System.Collections.Generic;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon.Events
{
    public class JudgingRunEmittedEvent : INotification
    {
        public IReadOnlyList<JudgingRun> Runs { get; }

        public Judging Judging { get; }

        public int? ContestId { get; }

        public int ProblemId { get; }

        public int TeamId { get; }

        public DateTimeOffset SubmitTime { get; }

        public int RankOfFirst { get; }

        public JudgingRunEmittedEvent(IReadOnlyList<JudgingRun> r, Judging j, int? cid, int probid, int uid, DateTimeOffset subtime, int rank0)
        {
            if (cid == 0) cid = null;
            Runs = r;
            Judging = j;
            ContestId = cid;
            ProblemId = probid;
            TeamId = uid;
            SubmitTime = subtime;
            RankOfFirst = rank0;
        }
    }
}
