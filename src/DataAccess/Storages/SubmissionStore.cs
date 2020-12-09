using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using Polygon.Events;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    public partial class PolygonFacade<TUser, TContext> : ISubmissionStore
    {
        DbSet<Submission> Submissions => Context.Set<Submission>();

        DbSet<SubmissionStatistics> SubmissionStatistics => Context.Set<SubmissionStatistics>();

        async Task<Submission> ISubmissionStore.CreateAsync(string code, string language, int problemId, int? contestId, int teamId, IPAddress ipAddr, string via, string username, Verdict? expected, DateTimeOffset? time, bool fullJudge)
        {
            var s = Submissions.Add(new Submission
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
            });

            await Context.SaveChangesAsync();

            bool fullTest = fullJudge || expected.HasValue;

            Judgings.Add(new Judging
            {
                SubmissionId = s.Entity.Id,
                FullTest = fullTest,
                Active = true,
                Status = Verdict.Pending,
            });

            await Context.SaveChangesAsync();
            await Mediator.Publish(new SubmissionCreatedEvent(s.Entity, via, username));
            return s.Entity;
        }

        Task<Submission> ISubmissionStore.FindAsync(int submissionId, bool includeJudgings)
        {
            return Submissions
                .Where(s => s.Id == submissionId)
                .IncludeIf(includeJudgings, s => s.Judgings)
                .SingleOrDefaultAsync();
        }

        Task<Submission> ISubmissionStore.FindByJudgingAsync(int jid)
        {
            return Judgings
                .Where(j => j.Id == jid)
                .Select(j => j.s)
                .SingleOrDefaultAsync();
        }

        Task<Dictionary<int, string>> ISubmissionStore.GetAuthorNamesAsync(Expression<Func<Submission, bool>> sids)
        {
            return Context.Author(sids).ToDictionaryAsync(s => s.SubmissionId, s => s.ToString());
        }

        Task<SubmissionFile> ISubmissionStore.GetFileAsync(int submissionId)
        {
            return Submissions
                .Where(s => s.Id == submissionId)
                .Select(s => new SubmissionFile(s.Id, s.SourceCode, s.l.FileExtension))
                .SingleOrDefaultAsync();
        }

        Task<List<T>> ISubmissionStore.ListAsync<T>(Expression<Func<Submission, T>> projection, Expression<Func<Submission, bool>>? predicate)
        {
            return Submissions
                .WhereIf(predicate != null, predicate)
                .Select(projection)
                .ToListAsync();
        }

        Task<IPagedList<T>> ISubmissionStore.ListWithJudgingAsync<T>((int Page, int PageCount) pagination, Expression<Func<Submission, Judging, T>> selector, Expression<Func<Submission, bool>>? predicate)
        {
            return Submissions
                .WhereIf(predicate != null, predicate!)
                .OrderByDescending(s => s.Id)
                .Join(
                    inner: Judgings,
                    outerKeySelector: s => new { s.Id, Active = true },
                    innerKeySelector: j => new { Id = j.SubmissionId, j.Active },
                    resultSelector: selector)
                .ToPagedListAsync(pagination.Page, pagination.PageCount);
        }

        Task<List<T>> ISubmissionStore.ListWithJudgingAsync<T>(Expression<Func<Submission, Judging, T>> selector, Expression<Func<Submission, bool>>? predicate, int? limits)
        {
            return Submissions
                .WhereIf(predicate != null, predicate!)
                .OrderByDescending(s => s.Id)
                .TakeIf(limits)
                .Join(
                    inner: Judgings,
                    outerKeySelector: s => new { s.Id, Active = true },
                    innerKeySelector: j => new { Id = j.SubmissionId, j.Active },
                    resultSelector: selector)
                .ToListAsync();
        }

        Task<List<SubmissionStatistics>> ISubmissionStore.StatisticsAsync(int contestId, int teamId)
        {
            return SubmissionStatistics
                .AsNoTracking()
                .Where(s => s.TeamId == teamId && s.ContestId == contestId)
                .ToListAsync();
        }

        Task ISubmissionStore.UpdateAsync(Submission entity) => UpdateEntityAsync(entity);

        Task ISubmissionStore.UpdateAsync(int id, Expression<Func<Submission, Submission>> expression)
        {
            return Submissions
                .Where(s => s.Id == id)
                .BatchUpdateAsync(expression);
        }

        Task ISubmissionStore.UpdateStatisticsAsync(int cid, int teamid, int probid, bool ac)
        {
            return SubmissionStatistics.MergeAsync(
                sourceTable: new[] { new { cid, teamid, probid, acc = ac ? 1 : 0 } },
                targetKey: s => new { teamid = s.TeamId, cid = s.ContestId, probid = s.ProblemId },
                sourceKey: s => new { s.teamid, s.cid, s.probid },
                delete: false,

                updateExpression: (s, s2) => new SubmissionStatistics
                {
                    AcceptedSubmission = s.AcceptedSubmission + s2.acc,
                    TotalSubmission = s.TotalSubmission + 1,
                },

                insertExpression: s2 => new SubmissionStatistics
                {
                    TeamId = s2.teamid,
                    ContestId = s2.cid,
                    ProblemId = s2.probid,
                    AcceptedSubmission = s2.acc,
                    TotalSubmission = 1,
                });
        }
    }
}
