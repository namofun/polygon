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
    public partial class PolygonFacade<TUser, TRole, TContext> : IProblemStore
    {
        DbSet<Problem> Problems => Context.Set<Problem>();

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
            var uidLimit = Context.Set<IdentityUserRole<int>>()
                .Where(ur => ur.UserId == uid)
                .Join(Context.Set<TRole>(), ur => ur.RoleId, r => r.Id, (ur, r) => r)
                .Where(r => r.ProblemId != null)
                .Select(r => (int)r.ProblemId!);

            return Problems
                .WhereIf(uid.HasValue, p => uidLimit.Contains(p.Id))
                .OrderBy(p => p.Id)
                .ToPagedListAsync(page, perCount);
        }

        async Task<IEnumerable<(int UserId, string UserName, string NickName)>> IProblemStore.ListPermittedUserAsync(int pid)
        {
            var result = await Context.Set<TRole>()
                .Where(r => r.ProblemId == pid)
                .Join(Context.Set<IdentityUserRole<int>>(), r => r.Id, ur => ur.RoleId, (r, ur) => ur)
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

            return SubmissionStatistics.MergeAsync(
                sourceTable: source,
                targetKey: ss => new { ss.TeamId, ss.ContestId, ss.ProblemId },
                sourceKey: ss => new { ss.TeamId, ss.ContestId, ss.ProblemId },
                delete: true,

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
    }
}
