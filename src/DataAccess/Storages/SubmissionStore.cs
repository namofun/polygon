using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Events;
using Xylab.Polygon.Models;

namespace Xylab.Polygon.Storages
{
    public partial class PolygonFacade<TContext, TQueryCache> : ISubmissionStore
    {
        async Task<Submission> ISubmissionStore.CreateAsync(string code, string language, int problemId, int? contestId, int teamId, IPAddress ipAddr, string via, string username, Verdict? expected, DateTimeOffset? time, bool fullJudge)
        {
            var s = Context.Submissions.Add(new Submission
            {
                TeamId = teamId,
                CodeLength = code.Length,
                ContestId = contestId ?? 0,
                Ip = ipAddr.ToString(),
                Language = language,
                ProblemId = problemId,
                Time = time ?? DateTimeOffset.Now,
                SourceCode = code.ToBase64(),
                ExpectedResult = expected,
                Judgings = new List<Judging>
                {
                    new Judging
                    {
                        FullTest = fullJudge || expected.HasValue,
                        Active = true,
                        Status = Verdict.Pending,
                        PolygonVersion = 1,
                    }
                },
            });

            await Context.SaveChangesAsync();
            await Mediator.Publish(new SubmissionCreatedEvent(s.Entity, via, username));
            return s.Entity;
        }

        Task<Submission?> ISubmissionStore.FindAsync(int submissionId, bool includeJudgings)
        {
            return Context.Submissions
                .Where(s => s.Id == submissionId)
                .IncludeIf(includeJudgings, s => s.Judgings)
                .SingleOrDefaultAsync();
        }

        Task<Submission?> ISubmissionStore.FindByJudgingAsync(int judgingid)
        {
            return Context.Judgings
                .Where(j => j.Id == judgingid)
                .Select(j => j.s)
                .SingleOrDefaultAsync();
        }

        async Task<Dictionary<int, string>> ISubmissionStore.GetAuthorNamesAsync(Expression<Func<Submission, bool>> submitids)
        {
            var result = await QueryCache.FetchSolutionAuthorAsync(Context, submitids);
            return result.ToDictionary(s => s.SubmissionId, s => s.ToString());
        }

        Task<SubmissionFile?> ISubmissionStore.GetFileAsync(int submissionId)
        {
            return Context.Submissions
                .Where(s => s.Id == submissionId)
                .Select(s => new SubmissionFile(s.Id, s.SourceCode, s.l.FileExtension))
                .SingleOrDefaultAsync();
        }

        Task<List<T>> ISubmissionStore.ListAsync<T>(Expression<Func<Submission, T>> projection, Expression<Func<Submission, bool>>? predicate, int? limit)
        {
            return Context.Submissions
                .WhereIf(predicate != null, predicate)
                .Select(projection)
                .TakeIf(limit)
                .ToListAsync();
        }

        Task<IPagedList<T>> ISubmissionStore.ListWithJudgingAsync<T>((int Page, int PageCount) pagination, Expression<Func<Submission, Judging, T>> selector, Expression<Func<Submission, bool>>? predicate)
        {
            return Context.Submissions
                .WhereIf(predicate != null, predicate!)
                .OrderByDescending(s => s.Time)
                .Join(
                    inner: Context.Judgings,
                    outerKeySelector: s => new { s.Id, Active = true },
                    innerKeySelector: j => new { Id = j.SubmissionId, j.Active },
                    resultSelector: selector)
                .ToPagedListAsync(pagination.Page, pagination.PageCount);
        }

        Task<IPagedList<T>> ISubmissionStore.ListWithJudgingAsync<T>((int Page, int PageCount) pagination, Expression<Func<Submission, Judging, T>> selector, Expression<Func<Submission, Judging, bool>>? predicate)
        {
            var template = new { s = (Submission)null!, j = (Judging)null! };
            var selector2 = selector.Combine(template, t => t.s, t => t.j)!;
            var predicate2 = predicate?.Combine(template, t => t.s, t => t.j);

            return Context.Submissions
                .OrderByDescending(s => s.Time)
                .Join(
                    inner: Context.Judgings,
                    outerKeySelector: s => new { s.Id, Active = true },
                    innerKeySelector: j => new { Id = j.SubmissionId, j.Active },
                    resultSelector: (s, j) => new { s, j })
                .WhereIf(predicate2 != null, predicate2)
                .Select(selector2)
                .ToPagedListAsync(pagination.Page, pagination.PageCount);
        }

        Task<List<T>> ISubmissionStore.ListWithJudgingAsync<T>(Expression<Func<Submission, Judging, T>> selector, Expression<Func<Submission, bool>>? predicate, int? limits)
        {
            return Context.Submissions
                .WhereIf(predicate != null, predicate!)
                .OrderByDescending(s => s.Time)
                .Join(
                    inner: Context.Judgings,
                    outerKeySelector: s => new { s.Id, Active = true },
                    innerKeySelector: j => new { Id = j.SubmissionId, j.Active },
                    resultSelector: selector)
                .TakeIf(limits)
                .ToListAsync();
        }

        Task<List<SubmissionStatistics>> ISubmissionStore.StatisticsAsync(int contestId, int teamId)
        {
            return Context.SubmissionStatistics
                .AsNoTracking()
                .Where(s => s.TeamId == teamId && s.ContestId == contestId)
                .ToListAsync();
        }

        async Task ISubmissionStore.UpdateAsync(Submission entity, Expression<Func<Submission, Submission>> expression)
        {
            int id = entity.Id;

            var ar = await Context.Submissions
                .Where(s => s.Id == id)
                .BatchUpdateAsync(expression);

            await Context.Entry(entity).ReloadAsync();
            await Mediator.Publish(new SubmissionModifiedEvent(entity));
        }

        Task ISubmissionStore.UpdateStatisticsAsync(int cid, int teamid, int probid, bool ac)
        {
            int ac_count = ac ? 1 : 0;

            return Context.SubmissionStatistics.UpsertAsync(

                insertExpression: () => new SubmissionStatistics
                {
                    TeamId = teamid,
                    ContestId = cid,
                    ProblemId = probid,
                    AcceptedSubmission = ac_count,
                    TotalSubmission = 1,
                },

                updateExpression: s => new SubmissionStatistics
                {
                    AcceptedSubmission = s.AcceptedSubmission + ac_count,
                    TotalSubmission = s.TotalSubmission + 1,
                });
        }
    }
}
