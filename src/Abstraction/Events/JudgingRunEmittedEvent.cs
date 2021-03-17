using MediatR;
using Polygon.Entities;
using System;
using System.Collections.Generic;

namespace Polygon.Events
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

        public JudgingRunEmittedEvent(IReadOnlyList<JudgingRun> r, Judging j, int? cid, int pid, int uid, DateTimeOffset subtime, int rank0)
        {
            if (cid == 0) cid = null;
            Runs = r;
            Judging = j;
            ContestId = cid;
            ProblemId = pid;
            TeamId = uid;
            SubmitTime = subtime;
            RankOfFirst = rank0;
        }
    }
}
