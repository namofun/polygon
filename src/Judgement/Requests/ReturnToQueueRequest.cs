﻿using MediatR;
using Polygon.Entities;
using Polygon.Events;
using System;

namespace Polygon.Judgement
{
    public class ReturnToQueueRequest : IRequest
    {
        public Judging Judging { get; }

        public int? ContestId { get; }

        public int ProblemId { get; }

        public int TeamId { get; }

        public DateTimeOffset SubmitTime { get; }

        public ReturnToQueueRequest(Judging j, int? cid, int pid, int uid, DateTimeOffset subtime)
        {
            if (cid == 0) cid = null;
            Judging = j;
            ContestId = cid;
            ProblemId = pid;
            TeamId = uid;
            SubmitTime = subtime;
        }

        public JudgingFinishedEvent ToEvent()
        {
            return new JudgingFinishedEvent(Judging, ContestId, ProblemId, TeamId, SubmitTime);
        }

        public UnfinishedJudging ToModel()
        {
            return new UnfinishedJudging
            {
                ContestId = ContestId ?? 0,
                JudgingId = Judging.Id,
                SubmissionId = Judging.SubmissionId
            };
        }
    }
}