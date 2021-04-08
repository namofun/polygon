using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Polygon.Entities;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    public partial class PolygonFacade<TContext, TQueryCache> : IJudgingStore
    {
        Task<Judging> IJudgingStore.CreateAsync(Judging entity) => CreateEntityAsync(entity);

        Task IJudgingStore.UpdateAsync(Judging entity) => UpdateEntityAsync(entity);

        Task IJudgingStore.UpdateAsync(int id, Expression<Func<Judging, Judging>> expression)
        {
            return Context.Judgings.Where(j => j.Id == id).BatchUpdateAsync(expression);
        }

        Task<int> IJudgingStore.CountAsync(Expression<Func<Judging, bool>> predicate)
        {
            return Context.Judgings.Where(predicate).CountAsync();
        }

        Task<T> IJudgingStore.FindAsync<T>(Expression<Func<Judging, bool>> predicate, Expression<Func<Judging, T>> selector)
        {
            return Context.Judgings.Where(predicate).OrderBy(j => j.Id).Select(selector).FirstOrDefaultAsync();
        }

        async Task<IEnumerable<T>> IJudgingStore.GetDetailsAsync<T>(int problemId, int judgingId, Expression<Func<Testcase, JudgingRun?, T>> selector)
        {
            var _selector = selector.Combine(
                objectTemplate1: new { t = default(Testcase)!, dd = default(IEnumerable<JudgingRun?>)! },
                objectTemplate2: default(JudgingRun),
                place1: (a, d) => a.t, place2: (a, d) => d);

            var query = Context.Testcases
                .Where(t => t.ProblemId == problemId)
                .OrderBy(t => t.Rank)
                .GroupJoin(
                    inner: (DbSet<JudgingRun?>)Context.JudgingRuns!,
                    outerKeySelector: t => new { TestcaseId = t.Id, JudgingId = judgingId },
                    innerKeySelector: d => new { d!.TestcaseId, d!.JudgingId },
                    resultSelector: (t, dd) => new { t, dd })
                .SelectMany(
                    collectionSelector: a => a.dd.DefaultIfEmpty(),
                    resultSelector: _selector);

            return await query.ToListAsync();
        }

        async Task<IEnumerable<T>> IJudgingStore.GetDetailsAsync<T>(Expression<Func<Testcase, JudgingRun, T>> selector, Expression<Func<Testcase, JudgingRun, bool>>? predicate, int? limit)
        {
            var _selector = selector.Combine(
                objectTemplate: new { t = default(Testcase)!, d = default(JudgingRun)! },
                place1: a => a.t, place2: a => a.d);
            var _predicate = predicate?.Combine(
                objectTemplate: new { t = default(Testcase)!, d = default(JudgingRun)! },
                place1: a => a.t, place2: a => a.d);

            return await Context.JudgingRuns
                .Join(Context.Testcases, d => d.TestcaseId, t => t.Id, (d, t) => new { t, d })
                .WhereIf(_predicate != null, _predicate)
                .OrderBy(a => a.d.Id)
                .Select(_selector)
                .TakeIf(limit)
                .ToListAsync();
        }

        async Task<List<ServerStatus>> IJudgingStore.GetJudgeQueueAsync(int? cid)
        {
            var judgingStatus = await Context.Submissions
                .WhereIf(cid.HasValue, s => s.ContestId == cid)
                .Join(
                    inner: Context.Judgings,
                    outerKeySelector: s => s.Id,
                    innerKeySelector: j => j.SubmissionId,
                    resultSelector: (s, j) => new { j.Status, s.ContestId })
                .GroupBy(g => new { g.Status, g.ContestId })
                .Select(g => new { g.Key.Status, g.Key.ContestId, Count = g.Count() })
                .ToListAsync();

            return judgingStatus
                .GroupBy(a => a.ContestId)
                .Select(g => new ServerStatus
                {
                    ContestId = g.Key,
                    Total = g.Sum(a => a.Count),
                    Queued = g.SingleOrDefault(a => a.Status == Verdict.Pending)?.Count ?? 0,
                    Judging = g.SingleOrDefault(a => a.Status == Verdict.Running)?.Count ?? 0,
                })
                .ToList();
        }

        Task<JudgingRun> IJudgingStore.InsertAsync(JudgingRun entity) => CreateEntityAsync(entity);

        Task<List<T>> IJudgingStore.ListAsync<T>(Expression<Func<Judging, bool>> predicate, Expression<Func<Judging, T>> selector, int topCount)
        {
            return Context.Judgings.Where(predicate).OrderByDescending(j => j.Id).Take(topCount).Select(selector).ToListAsync();
        }

        Task<List<Judging>> IJudgingStore.ListAsync(Expression<Func<Judging, bool>> predicate, int topCount)
        {
            return Context.Judgings.Where(predicate).OrderBy(j => j.Id).Take(topCount).ToListAsync();
        }

        async Task<IFileInfo> IJudgingStore.GetRunFileAsync(int jid, int rid, string type, int? sid, int? pid)
        {
            var notfound = new NotFoundFileInfo($"j{jid}/r{rid}.{type}");
            var fileInfo = await JudgingFiles.GetFileInfoAsync($"j{jid}/r{rid}.{type}");
            if (!fileInfo.Exists) return notfound;

            if (sid.HasValue || pid.HasValue)
            {
                var validation = await Context.JudgingRuns
                    .Where(r => r.JudgingId == jid && r.Id == rid)
                    .Select(r => new { SubmissionId = r.j.s.Id, r.j.s.ProblemId })
                    .SingleOrDefaultAsync();
                if (sid.HasValue && validation.SubmissionId != sid.Value) return notfound;
                if (pid.HasValue && validation.ProblemId != pid.Value) return notfound;
            }

            return fileInfo;
        }

        Task<IFileInfo> IJudgingStore.SetRunFileAsync(int jid, int rid, string type, byte[] content)
        {
            return JudgingFiles.WriteBinaryAsync($"j{jid}/r{rid}.{type}", content);
        }

        Task<RunSummary> IJudgingStore.SummarizeAsync(int judgingId)
        {
            var query =
                from d in Context.JudgingRuns
                where d.JudgingId == judgingId
                join t in Context.Testcases on d.TestcaseId equals t.Id
                group new { d.Status, d.ExecuteMemory, d.ExecuteTime, t.Point } by d.JudgingId into g
                select new RunSummary
                {
                    JudgingId = g.Key,
                    FinalVerdict = g.Min(a => a.Status),
                    Testcases = g.Count(),
                    HighestMemory = g.Max(a => a.ExecuteMemory),
                    LongestTime = g.Max(a => a.ExecuteTime),
                    TotalScore = g.Sum(a => a.Status == Verdict.Accepted ? a.Point : 0)
                };

            return query.FirstOrDefaultAsync();
        }

        Task<JudgingRun?> IJudgingStore.GetDetailAsync(int problemId, int submitId, int judgingId, int runId)
        {
            return Context.JudgingRuns
                .Where(jr => jr.Id == runId && jr.j.Id == judgingId)
                .Where(jr => jr.j.s.Id == submitId && jr.j.s.ProblemId == problemId)
                .FirstOrDefaultAsync()!;
        }
    }
}
