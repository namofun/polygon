using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Polygon.Entities;
using Polygon.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    public partial class PolygonFacade<TContext, TQueryCache> : IProblemStore
    {
        Task<Problem> IProblemStore.CreateAsync(Problem entity) => CreateEntityAsync(entity);

        Task IProblemStore.DeleteAsync(Problem entity) => DeleteEntityAsync(entity);

        Task IProblemStore.CommitChangesAsync(Problem entity) => UpdateEntityAsync(entity);

        async Task IProblemStore.UpdateAsync(Problem problem, Expression<Func<Problem, Problem>> expression)
        {
            int id = problem.Id;

            await Context.Problems
                .Where(p => p.Id == id)
                .BatchUpdateAsync(expression);

            await Context.Entry(problem).ReloadAsync();
        }

        Task<Problem?> IProblemStore.FindAsync(int probid)
        {
            return Context.Problems
                .Where(p => p.Id == probid)
                .SingleOrDefaultAsync();
        }

        Task<IBlobInfo> IProblemStore.GetFileAsync(int problemId, string fileName)
        {
            return ProblemFiles.GetFileInfoAsync($"p{problemId}/{fileName}");
        }

        Task<IPagedList<Problem>> IProblemStore.ListAsync(int page, int perCount, bool ascending, int? uid, AuthorLevel? leastLevel)
        {
            if (uid.HasValue)
                return Context.ProblemAuthors
                    .Where(pa => pa.UserId == uid)
                    .WhereIf(leastLevel.HasValue, pa => pa.Level >= leastLevel)
                    .OrderByBoolean(pa => pa.ProblemId, ascending)
                    .Join(Context.Problems, pa => pa.ProblemId, p => p.Id, (pa, p) => p)
                    .ToPagedListAsync(page, perCount);
            else
                return Context.Problems
                    .OrderByBoolean(p => p.Id, ascending)
                    .ToPagedListAsync(page, perCount);
        }

        Task<IEnumerable<(int UserId, string UserName, AuthorLevel Level)>> IProblemStore.ListPermittedUserAsync(int probid)
        {
            return QueryCache.FetchPermittedUserAsync(Context, probid);
        }

        Task IProblemStore.RebuildStatisticsAsync()
        {
            var source =
                from s in Context.Submissions
                join j in Context.Judgings on new { s.Id, Active = true } equals new { Id = j.SubmissionId, j.Active }
                group j.Status by new { s.ProblemId, s.TeamId, s.ContestId } into g
                select new SubmissionStatistics
                {
                    ProblemId = g.Key.ProblemId,
                    TeamId = g.Key.TeamId,
                    ContestId = g.Key.ContestId,
                    TotalSubmission = g.Count(),
                    AcceptedSubmission = g.Sum(v => v == Verdict.Accepted ? 1 : 0)
                };

            return Context.SubmissionStatistics.UpsertAsync(
                sources: source,

                updateExpression: (_, ss) => new SubmissionStatistics
                {
                    AcceptedSubmission = ss.AcceptedSubmission,
                    TotalSubmission = ss.TotalSubmission
                },

                insertExpression: ss => new SubmissionStatistics
                {
                    TeamId = ss.TeamId,
                    ContestId = ss.ContestId,
                    ProblemId = ss.ProblemId,
                    AcceptedSubmission = ss.AcceptedSubmission,
                    TotalSubmission = ss.TotalSubmission
                });
        }

        Task IProblemStore.ToggleJudgeAsync(int probid, bool tobe)
        {
            return Context.Problems
                .Where(p => p.Id == probid)
                .BatchUpdateAsync(p => new Problem { AllowJudge = tobe });
        }

        Task IProblemStore.ToggleSubmitAsync(int probid, bool tobe)
        {
            return Context.Problems
                .Where(p => p.Id == probid)
                .BatchUpdateAsync(p => new Problem { AllowSubmit = tobe });
        }

        Task<IBlobInfo> IProblemStore.WriteFileAsync(Problem problem, string fileName, string content)
        {
            return ProblemFiles.WriteStringAsync($"p{problem.Id}/{fileName}", content);
        }

        Task<IBlobInfo> IProblemStore.WriteFileAsync(Problem problem, string fileName, byte[] content)
        {
            return ProblemFiles.WriteBinaryAsync($"p{problem.Id}/{fileName}", content);
        }

        Task<IBlobInfo> IProblemStore.WriteFileAsync(Problem problem, string fileName, Stream content)
        {
            return ProblemFiles.WriteStreamAsync($"p{problem.Id}/{fileName}", content);
        }

        Task<Dictionary<int, string>> IProblemStore.ListNameAsync(Expression<Func<Problem, bool>> condition)
        {
            return Context.Problems
                .Where(condition)
                .Select(p => new { p.Id, p.Title })
                .ToDictionaryAsync(p => p.Id, p => p.Title);
        }

        Task<Dictionary<int, string>> IProblemStore.ListNameAsync(Expression<Func<Submission, bool>> condition)
        {
            return Context.Submissions
                .Where(condition)
                .Join(Context.Problems, s => s.ProblemId, p => p.Id, (s, p) => new { p.Id, p.Title })
                .Distinct()
                .ToDictionaryAsync(p => p.Id, p => p.Title);
        }

        async Task<string?> IProblemStore.ReadCompiledHtmlAsync(int problemId)
        {
            var fileInfo = await ((IProblemStore)this).GetFileAsync(problemId, "view.html");
            return await fileInfo.ReadAsync();
        }

        Task IProblemStore.AuthorizeAsync(int problemId, int userId, AuthorLevel? level)
        {
            if (level == null)
            {
                return Context.ProblemAuthors
                    .Where(pa => pa.ProblemId == problemId && pa.UserId == userId)
                    .BatchDeleteAsync();
            }
            else
            {
                return Context.ProblemAuthors
                    .UpsertAsync(() => new ProblemAuthor
                    {
                        ProblemId = problemId,
                        UserId = userId,
                        Level = level.Value,
                    });
            }
        }

        async Task<(Problem?, AuthorLevel?)> IProblemStore.FindAsync(int problemId, int userId)
        {
            var res = await Context.ProblemAuthors
                .Where(pa => pa.ProblemId == problemId && pa.UserId == userId)
                .Join(Context.Problems, pa => pa.ProblemId, p => p.Id, (pa, p) => new { pa.Level, p })
                .SingleOrDefaultAsync();

            return res == null ? default : (res.p, res.Level);
        }

        async Task<AuthorLevel?> IProblemStore.CheckPermissionAsync(int problemId, int userId)
        {
            var result = await Context.ProblemAuthors
                .Where(pa => pa.ProblemId == problemId && pa.UserId == userId)
                .Select(pa => new { pa.Level })
                .FirstOrDefaultAsync();

            return result?.Level;
        }

        async Task<IEnumerable<(int, string)>> IProblemStore.ListPermissionAsync(int userId)
        {
            var items = await Context.ProblemAuthors
                .Where(pa => pa.UserId == userId)
                .Join(Context.Problems, pa => pa.ProblemId, p => p.Id, (pa, p) => p)
                .Select(p => new { p.Id, p.Title })
                .ToListAsync();

            return items.Select(a => (a.Id, a.Title));
        }
    }
}
