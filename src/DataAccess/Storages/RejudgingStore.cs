using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    public partial class PolygonFacade<TUser, TContext> : IRejudgingStore
    {
        private static readonly ConcurrentAsyncLock _beingRejudged = new ConcurrentAsyncLock();

        DbSet<Rejudging> Rejudgings => Context.Set<Rejudging>();

        async Task IRejudgingStore.ApplyAsync(Rejudging rejudge, int uid)
        {
            int rid = rejudge.Id;
            var applyNew = await Judgings
                .Where(j => j.RejudgingId == rid)
                .BatchUpdateAsync(j => new Judging { Active = true });

            var oldJudgings = Judgings
                .Where(j => j.RejudgingId == rid)
                .Select(j => j.PreviousJudgingId);
            var supplyOld = await Judgings
                .Where(j => oldJudgings.Contains(j.Id))
                .BatchUpdateAsync(j => new Judging { Active = false });

            var oldSubmissions = Judgings
                .Where(j => j.RejudgingId == rid)
                .Select(j => j.SubmissionId);
            var resetSubmit = await Submissions
                .Where(s => oldSubmissions.Contains(s.Id))
                .BatchUpdateAsync(s => new Submission { RejudgingId = null });

            await Rejudgings
                .Where(r => r.Id == rid)
                .BatchUpdateAsync(r => new Rejudging
                {
                    Applied = true,
                    EndTime = DateTimeOffset.Now,
                    OperatedBy = uid,
                });

            var statisticsMerge =
                from j in Judgings
                where j.RejudgingId == rid
                join j2 in Judgings on j.PreviousJudgingId equals j2.Id
                join s in Submissions on j.SubmissionId equals s.Id
                where (j.Status == Verdict.Accepted && j2.Status != Verdict.Accepted) || (j.Status != Verdict.Accepted && j2.Status == Verdict.Accepted)
                group j.Status == Verdict.Accepted ? 1 : -1 by new { s.ContestId, s.TeamId, s.ProblemId } into g
                select new { g.Key.TeamId, g.Key.ContestId, g.Key.ProblemId, Delta = g.Sum() };

            await Context.Set<SubmissionStatistics>()
                .BatchUpdateJoinAsync(
                    inner: statisticsMerge,
                    outerKeySelector: s => new { s.TeamId, s.ContestId, s.ProblemId },
                    innerKeySelector: s => new { s.TeamId, s.ContestId, s.ProblemId },
                    updateSelector: (o, s) => new SubmissionStatistics
                    {
                        AcceptedSubmission = o.AcceptedSubmission + s.Delta
                    });
        }

        async Task<int> IRejudgingStore.BatchRejudgeAsync(Expression<Func<Submission, Judging, bool>> predicate, Rejudging? rejudge, bool fullTest)
        {
            int cid = rejudge?.ContestId ?? 0;
            var selectionQuery = Submissions
                .Where(s => s.ContestId == cid && s.RejudgingId == null)
                .Join(Judgings, s => s.Id, j => j.SubmissionId, (s, j) => new { s, j });

            var _predicate = predicate.Combine(
                objectTemplate: new { s = default(Submission)!, j = default(Judging)! },
                place1: a => a.s, place2: a => a.j);
            selectionQuery = selectionQuery.Where(_predicate);

            var sublist_0 = selectionQuery.Select(a => a.s.Id).Distinct();
            var sublist = Submissions.Where(s => sublist_0.Contains(s.Id));

            if (rejudge == null)
            {
                // TODO: Optimize here
                var items = await sublist.ToListAsync();
                foreach (var sub in items)
                    await ((IRejudgingStore)this).RejudgeAsync(sub, fullTest);
                return items.Count;
            }
            else
            {
                int rejid = rejudge.Id;
                int count = await sublist.BatchUpdateAsync(s => new Submission { RejudgingId = rejid });
                if (count == 0) return 0;

                return await Submissions
                    .Where(s => s.RejudgingId == rejid)
                    .Join(
                        inner: Judgings,
                        outerKeySelector: s => s.Id,
                        innerKeySelector: j => j.SubmissionId,
                        resultSelector: (s, j) => new Judging
                        {
                            Active = false,
                            SubmissionId = s.Id,
                            FullTest = fullTest ? true : j.FullTest,
                            Status = Verdict.Pending,
                            RejudgingId = rejid,
                            PreviousJudgingId = j.Id,
                        })
                    .BatchInsertIntoAsync(Judgings);
            }
        }

        async Task IRejudgingStore.CancelAsync(Rejudging rejudge, int uid)
        {
            int rid = rejudge.Id;

            var cancelJudgings = await Judgings
                .Where(j => j.RejudgingId == rid && j.Status == Verdict.Pending)
                .BatchDeleteAsync();
            var resetSubmits = await Submissions
                .Where(s => s.RejudgingId == rid)
                .BatchUpdateAsync(s => new Submission { RejudgingId = null });

            await Rejudgings
                .Where(r => r.Id == rid)
                .BatchUpdateAsync(r => new Rejudging
                {
                    EndTime = DateTimeOffset.Now,
                    Applied = false,
                    OperatedBy = uid
                });
        }

        Task<int> IRejudgingStore.CountUndoneAsync(int cid)
        {
            return Rejudgings
                .Where(t => t.Applied == null && t.ContestId == cid)
                .CountAsync();
        }

        Task<Rejudging> IRejudgingStore.CreateAsync(Rejudging entity) => CreateEntityAsync(entity);

        Task IRejudgingStore.DeleteAsync(Rejudging entity) => DeleteEntityAsync(entity);

        Task<Rejudging> IRejudgingStore.FindAsync(int cid, int rjid)
        {
            return Rejudgings
                .Where(r => r.ContestId == cid && r.Id == rjid)
                .SingleOrDefaultAsync();
        }

        async Task<List<Rejudging>> IRejudgingStore.ListAsync(int contestId, bool includeStat)
        {
            var model = await Rejudgings
                .Where(r => r.ContestId == contestId)
                .ToListAsync();

            if (includeStat)
            {
                var query2 =
                    from j in Judgings
                    where (from r in Rejudgings
                           where r.ContestId == contestId && r.OperatedBy == null
                           select (int?)r.Id).Contains(j.RejudgingId)
                    group 1 by new { j.RejudgingId, j.Status } into g
                    select new { g.Key, Cnt = g.Count() };
                var q2 = await query2.ToListAsync();

                foreach (var qqq in q2.GroupBy(a => a.Key.RejudgingId))
                {
                    int tot = qqq.Sum(a => a.Cnt);
                    int ped = qqq
                        .Where(a => a.Key.Status == Verdict.Pending || a.Key.Status == Verdict.Running)
                        .DefaultIfEmpty()
                        .Sum(a => a?.Cnt) ?? 0;
                    model.First(r => r.Id == qqq.Key).Ready = (tot, ped);
                }
            }

            return model;
        }

        async Task IRejudgingStore.RejudgeAsync(Submission sub, bool fullTest)
        {
            if (sub.ExpectedResult != null) fullTest = true;
            Judging currentJudging;

            using (await _beingRejudged.LockAsync(sub.Id))
            {
                currentJudging = await Judgings
                    .Where(j => j.SubmissionId == sub.Id && j.Active)
                    .SingleAsync();

                currentJudging.Active = false;
                fullTest = fullTest || currentJudging.FullTest;
                Judgings.Update(currentJudging);

                Judgings.Add(new Judging
                {
                    SubmissionId = sub.Id,
                    FullTest = fullTest,
                    Active = true,
                    Status = Verdict.Pending,
                    PreviousJudgingId = currentJudging.Id,
                });

                await Context.SaveChangesAsync();
            }

            var (cid, tid, pid, acc) = (
                sub.ContestId, sub.TeamId, sub.ProblemId,
                currentJudging.Status == Verdict.Accepted ? 1 : 0);

            await Context.Set<SubmissionStatistics>()
                .Where(s => s.TeamId == tid && s.ContestId == cid && s.ProblemId == pid)
                .BatchUpdateAsync(s => new SubmissionStatistics
                {
                    TotalSubmission = s.TotalSubmission - 1,
                    AcceptedSubmission = s.AcceptedSubmission - acc,
                });
        }

        Task IRejudgingStore.UpdateAsync(Rejudging entity) => UpdateEntityAsync(entity);

        Task IRejudgingStore.UpdateAsync(int id, Expression<Func<Rejudging, Rejudging>> expression)
        {
            return Rejudgings.Where(r => r.Id == id).BatchUpdateAsync(expression);
        }

        async Task<IEnumerable<RejudgingDifference>> IRejudgingStore.ViewAsync(Rejudging rejudge, Expression<Func<Judging, Judging, Submission, bool>>? filter)
        {
            var query =
                from j in Judgings
                where j.RejudgingId == rejudge.Id
                join s in Submissions on j.SubmissionId equals s.Id
                join j2 in Judgings on j.PreviousJudgingId equals j2.Id
                orderby s.Time descending
                select new RejudgingDifference(j2, j, s.ProblemId, s.Language, s.Time, s.TeamId, s.ContestId);
            return await query.ToListAsync();
        }
    }
}
