using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    public partial class PolygonFacade<TUser, TContext> : IProblemStore
    {
        DbSet<Problem> Problems => Context.Set<Problem>();

        DbSet<ProblemAuthor> Authors => Context.Set<ProblemAuthor>();

        Task<Problem> IProblemStore.CreateAsync(Problem entity) => CreateEntityAsync(entity);

        Task IProblemStore.DeleteAsync(Problem entity) => DeleteEntityAsync(entity);

        Task IProblemStore.UpdateAsync(Problem entity) => UpdateEntityAsync(entity);

        Task IProblemStore.UpdateAsync(int id, Expression<Func<Problem, Problem>> expression) => Problems.Where(p => p.Id == id).BatchUpdateAsync(expression);

        Task<Problem> IProblemStore.FindAsync(int pid)
        {
            return Problems
                .Where(p => p.Id == pid)
                .SingleOrDefaultAsync();
        }

        Task<IFileInfo> IProblemStore.GetFileAsync(int problemId, string fileName)
        {
            return ProblemFiles.GetFileInfoAsync($"p{problemId}/{fileName}");
        }

        Task<IPagedList<Problem>> IProblemStore.ListAsync(int page, int perCount, int? uid)
        {
            if (uid.HasValue)
                return Authors
                    .Where(pa => pa.UserId == uid)
                    .OrderBy(pa => pa.ProblemId)
                    .Join(Problems, pa => pa.ProblemId, p => p.Id, (pa, p) => p)
                    .ToPagedListAsync(page, perCount);
            else
                return Problems
                    .OrderBy(p => p.Id)
                    .ToPagedListAsync(page, perCount);
        }

        async Task<IEnumerable<(int UserId, string UserName, string NickName)>> IProblemStore.ListPermittedUserAsync(int pid)
        {
#warning Shouldn't join TUser
            var result = await Authors
                .Where(r => r.ProblemId == pid)
                .Join(Context.Set<TUser>(), ur => ur.UserId, u => u.Id, (ur, u) => new { u.Id, u.UserName, u.NickName })
                .ToListAsync();
            return result.Select(a => (a.Id, a.UserName, a.NickName));
        }

        Task IProblemStore.RebuildStatisticsAsync()
        {
            var source =
                from s in Submissions
                join j in Judgings on new { s.Id, Active = true } equals new { Id = j.SubmissionId, j.Active }
                group j.Status by new { s.ProblemId, s.TeamId, s.ContestId } into g
                select new SubmissionStatistics
                {
                    ProblemId = g.Key.ProblemId,
                    TeamId = g.Key.TeamId,
                    ContestId = g.Key.ContestId,
                    TotalSubmission = g.Count(),
                    AcceptedSubmission = g.Sum(v => v == Verdict.Accepted ? 1 : 0)
                };

            return SubmissionStatistics.UpsertAsync(
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

        Task IProblemStore.ToggleJudgeAsync(int pid, bool tobe)
        {
            return Problems
                .Where(p => p.Id == pid)
                .BatchUpdateAsync(p => new Problem { AllowJudge = tobe });
        }

        Task IProblemStore.ToggleSubmitAsync(int pid, bool tobe)
        {
            return Problems
                .Where(p => p.Id == pid)
                .BatchUpdateAsync(p => new Problem { AllowSubmit = tobe });
        }

        Task<IFileInfo> IProblemStore.WriteFileAsync(Problem problem, string fileName, string content)
        {
            return ProblemFiles.WriteStringAsync($"p{problem.Id}/{fileName}", content);
        }

        Task<IFileInfo> IProblemStore.WriteFileAsync(Problem problem, string fileName, byte[] content)
        {
            return ProblemFiles.WriteBinaryAsync($"p{problem.Id}/{fileName}", content);
        }

        Task<IFileInfo> IProblemStore.WriteFileAsync(Problem problem, string fileName, Stream content)
        {
            return ProblemFiles.WriteStreamAsync($"p{problem.Id}/{fileName}", content);
        }

        Task<Dictionary<int, string>> IProblemStore.ListNameAsync(Expression<Func<Problem, bool>> condition)
        {
            return Problems
                .Where(condition)
                .Select(p => new { p.Id, p.Title })
                .ToDictionaryAsync(p => p.Id, p => p.Title);
        }

        Task<Dictionary<int, string>> IProblemStore.ListNameAsync(Expression<Func<Submission, bool>> condition)
        {
            return Submissions
                .Where(condition)
                .Join(Problems, s => s.ProblemId, p => p.Id, (s, p) => p)
                .Select(p => new { p.Id, p.Title })
                .Distinct()
                .ToDictionaryAsync(p => p.Id, p => p.Title);
        }

        async Task<string?> IProblemStore.ReadCompiledHtmlAsync(int problemId)
        {
            var fileInfo = await ((IProblemStore)this).GetFileAsync(problemId, "view.html");
            return await fileInfo.ReadAsync();
        }

        async Task IProblemStore.AuthorizeAsync(int problemId, int userId, bool allow)
        {
            var auth = await Authors
                .Where(pa => pa.ProblemId == problemId && pa.UserId == userId)
                .FirstOrDefaultAsync();

            if (allow && auth == null)
            {
                await CreateEntityAsync(new ProblemAuthor
                {
                    ProblemId = problemId,
                    UserId = userId
                });
            }
            else if (!allow && auth != null)
            {
                await DeleteEntityAsync(auth);
            }
        }

        Task<Problem> IProblemStore.FindByPermissionAsync(int problemId, int userId)
        {
            return Authors
                .Where(pa => pa.ProblemId == problemId && pa.UserId == userId)
                .Join(Problems, pa => pa.ProblemId, p => p.Id, (pa, p) => p)
                .SingleOrDefaultAsync();
        }
    }
}
