using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Events;
using Xylab.Polygon.Models;

namespace Xylab.Polygon.Storages
{
    public partial class PolygonFacade<TContext, TQueryCache> : IRejudgingStore
    {
        async Task IRejudgingStore.ApplyAsync(Rejudging rejudge, int uid)
        {
            int rejid = rejudge.Id;
            var applyNew = await Context.Judgings
                .Where(j => j.RejudgingId == rejid)
                .BatchUpdateAsync(j => new Judging { Active = true });

            var oldJudgings = Context.Judgings
                .Where(j => j.RejudgingId == rejid)
                .Select(j => j.PreviousJudgingId);
            var supplyOld = await Context.Judgings
                .Where(j => oldJudgings.Contains(j.Id))
                .BatchUpdateAsync(j => new Judging { Active = false });

            var oldSubmissions = Context.Judgings
                .Where(j => j.RejudgingId == rejid)
                .Select(j => j.SubmissionId);
            var resetSubmit = await Context.Submissions
                .Where(s => oldSubmissions.Contains(s.Id))
                .BatchUpdateAsync(s => new Submission { RejudgingId = null });

            await Context.Rejudgings
                .Where(r => r.Id == rejid)
                .BatchUpdateAsync(r => new Rejudging
                {
                    Applied = true,
                    EndTime = DateTimeOffset.Now,
                    OperatedBy = uid,
                });

            var statisticsMerge =
                from j in Context.Judgings
                where j.RejudgingId == rejid
                join j2 in Context.Judgings on j.PreviousJudgingId equals j2.Id
                join s in Context.Submissions on j.SubmissionId equals s.Id
                where (j.Status == Verdict.Accepted && j2.Status != Verdict.Accepted) || (j.Status != Verdict.Accepted && j2.Status == Verdict.Accepted)
                group j.Status == Verdict.Accepted ? 1 : -1 by new { s.ContestId, s.TeamId, s.ProblemId } into g
                select new { g.Key.TeamId, g.Key.ContestId, g.Key.ProblemId, Delta = g.Sum() };

            await Context.SubmissionStatistics.BatchUpdateJoinAsync(
                inner: statisticsMerge,
                outerKeySelector: s => new { s.TeamId, s.ContestId, s.ProblemId },
                innerKeySelector: s => new { s.TeamId, s.ContestId, s.ProblemId },
                updateSelector: (o, s) => new SubmissionStatistics
                {
                    AcceptedSubmission = o.AcceptedSubmission + s.Delta
                });

            await Mediator.Publish(new RejudgingAppliedEvent(rejudge));
        }

        async Task<int> IRejudgingStore.BatchRejudgeAsync(
            Expression<Func<Submission, Judging, bool>> predicate,
            Rejudging rejudging,
            bool fullTest,
            bool immediateApply,
            bool stageAsRunning)
        {
            int cid = rejudging.ContestId ?? 0;
            var selectionQuery = Context.Submissions
                .Where(s => s.ContestId == cid && s.RejudgingId == null)
                .Join(
                    inner: Context.Judgings,
                    outerKeySelector: s => new { s.Id, Active = true },
                    innerKeySelector: j => new { Id = j.SubmissionId, j.Active },
                    resultSelector: (s, j) => new { s, j })
                .Where(predicate.Combine(new { s = default(Submission)!, j = default(Judging)! }, a => a.s, a => a.j))
                .Select(a => a.s.Id);

            int rejid = rejudging.Id;
            int count = await Context.Submissions
                .Where(s => selectionQuery.Contains(s.Id))
                .BatchUpdateAsync(s => new Submission { RejudgingId = rejid });

            if (count == 0) return 0;

            Verdict targetVerdict = stageAsRunning ? Verdict.Running : Verdict.Pending;
            var newRj = await Context.Submissions
                .Where(s => s.RejudgingId == rejid)
                .Join(
                    inner: Context.Judgings,
                    outerKeySelector: s => new { s.Id, Active = true },
                    innerKeySelector: j => new { Id = j.SubmissionId, j.Active },
                    resultSelector: (s, j) => new Judging
                    {
                        Active = false,
                        SubmissionId = s.Id,
                        FullTest = fullTest ? true : j.FullTest,
                        Status = targetVerdict,
                        RejudgingId = rejid,
                        PreviousJudgingId = j.Id,
                        PolygonVersion = 1,
                    })
                .BatchInsertIntoAsync(Context.Judgings);

            if (immediateApply)
            {
                await Context.SubmissionStatistics.BatchUpdateJoinAsync(
                    inner: from s in Context.Submissions
                           where s.RejudgingId == rejid
                           join j in Context.Judgings
                                  on new { s.Id, Active = true }
                                  equals new { Id = j.SubmissionId, j.Active }
                           select new { s.ContestId, s.ProblemId, s.TeamId, j.Status },
                    outerKeySelector: s => new { s.ContestId, s.TeamId, s.ProblemId },
                    innerKeySelector: s => new { s.ContestId, s.TeamId, s.ProblemId },
                    updateSelector: (stat, inner) => new()
                    {
                        AcceptedSubmission = stat.AcceptedSubmission - (inner.Status == Verdict.Accepted ? 1 : 0),
                        TotalSubmission = stat.TotalSubmission - (inner.Status < Verdict.Pending ? 1 : 0),
                    });

                await Context.Judgings
                    .Where(j => j.s.RejudgingId == rejid && (j.Active || j.RejudgingId == rejid))
                    .BatchUpdateAsync(j => new Judging { Active = j.RejudgingId == rejid });

                await Context.Submissions
                    .Where(s => s.RejudgingId == rejid)
                    .BatchUpdateAsync(s => new() { RejudgingId = null });
            }

            return newRj;
        }

        async Task IRejudgingStore.CancelAsync(Rejudging rejudge, int uid)
        {
            int rejid = rejudge.Id;

            var cancelJudgings = await Context.Judgings
                .Where(j => j.RejudgingId == rejid && j.Status == Verdict.Pending)
                .BatchDeleteAsync();
            var resetSubmits = await Context.Submissions
                .Where(s => s.RejudgingId == rejid)
                .BatchUpdateAsync(s => new Submission { RejudgingId = null });

            await Context.Rejudgings
                .Where(r => r.Id == rejid)
                .BatchUpdateAsync(r => new Rejudging
                {
                    EndTime = DateTimeOffset.Now,
                    Applied = false,
                    OperatedBy = uid
                });
        }

        Task<int> IRejudgingStore.CountUndoneAsync(int cid)
        {
            return Context.Rejudgings
                .Where(t => t.Applied == null && t.ContestId == cid)
                .CountAsync();
        }

        Task<Rejudging> IRejudgingStore.CreateAsync(Rejudging entity) => CreateEntityAsync(entity);

        Task IRejudgingStore.DeleteAsync(Rejudging entity) => DeleteEntityAsync(entity);

        Task<Rejudging?> IRejudgingStore.FindAsync(int cid, int rejid)
        {
            return Context.Rejudgings
                .Where(r => r.ContestId == cid && r.Id == rejid)
                .SingleOrDefaultAsync();
        }

        async Task<List<Rejudging>> IRejudgingStore.ListAsync(int contestId, bool includeStat)
        {
            var model = await Context.Rejudgings
                .Where(r => r.ContestId == contestId)
                .ToListAsync();

            if (includeStat)
            {
                var query3 =
                    from r in Context.Rejudgings
                    where r.ContestId == contestId && r.OperatedBy == null
                    join j in Context.Judgings on r.Id equals j.RejudgingId
                    group j by new { j.RejudgingId, j.Status } into g
                    select new { RejudgingId = (int)g.Key.RejudgingId!, g.Key.Status, Count = g.Count() };

                foreach (var stat in await query3.ToLookupAsync(a => a.RejudgingId, a => a))
                {
                    int tot = stat.Sum(a => a.Count);
                    int ped = stat.Sum(a => a.Status == Verdict.Pending || a.Status == Verdict.Running ? 1 : 0);
                    model.First(r => r.Id == stat.Key).Ready = (tot, ped);
                }
            }

            return model;
        }

        async Task<IEnumerable<RejudgingDifference>> IRejudgingStore.ViewAsync(Rejudging rejudge, Expression<Func<Judging, Judging, Submission, bool>>? filter)
        {
            var query =
                from j in Context.Judgings
                where j.RejudgingId == rejudge.Id
                join s in Context.Submissions on j.SubmissionId equals s.Id
                join j2 in Context.Judgings on j.PreviousJudgingId equals j2.Id
                orderby s.Time descending
                select new RejudgingDifference(j2, j, s.ProblemId, s.Language, s.Time, s.TeamId, s.ContestId);

            return await query.ToListAsync();
        }
    }
}
