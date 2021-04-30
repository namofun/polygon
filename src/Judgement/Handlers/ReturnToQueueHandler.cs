﻿using MediatR;
using Polygon.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polygon.Judgement
{
    public partial class DOMjudgeLikeHandlers : IRequestHandler<ReturnToQueueRequest>
    {
        public async Task<Unit> Handle(ReturnToQueueRequest request, CancellationToken cancellationToken)
        {
            var j = request.Judging;

            await Facade.Judgings.CreateAsync(
                new Judging
                {
                    Active = j.Active,
                    Status = Verdict.Pending,
                    FullTest = j.FullTest,
                    RejudgingId = j.RejudgingId,
                    PreviousJudgingId = j.PreviousJudgingId,
                    SubmissionId = j.SubmissionId,
                    PolygonVersion = 1,
                });

            j.Active = false;
            j.Status = Verdict.UndefinedError;
            j.RejudgingId = null;
            j.PreviousJudgingId = null;
            if (!j.StopTime.HasValue)
                j.StopTime = DateTimeOffset.Now;

            await Facade.Judgings.UpdateAsync(j);
            await FinalizeJudging(request.ToEvent());
            return Unit.Value;
        }
    }
}
