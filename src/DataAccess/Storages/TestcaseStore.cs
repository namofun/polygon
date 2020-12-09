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
    public partial class PolygonFacade<TUser, TContext> : ITestcaseStore
    {
        DbSet<Testcase> Testcases => Context.Set<Testcase>();

        Task<int> ITestcaseStore.BatchScoreAsync(int pid, int lower, int upper, int score)
        {
            return Testcases
                .Where(t => t.ProblemId == pid && t.Rank >= lower && t.Rank <= upper)
                .BatchUpdateAsync(t => new Testcase { Point = score });
        }

        async Task<int> ITestcaseStore.CascadeDeleteAsync(Testcase testcase)
        {
            using var tran = await Context.Database.BeginTransactionAsync();
            int dts;

            try
            {
                // details are set ON DELETE NO ACTION, so we have to delete it before
                dts = await Details.Where(d => d.TestcaseId == testcase.Id).BatchDeleteAsync();
                await Testcases.Where(t => t.Id == testcase.Id).BatchDeleteAsync();
                // set the rest testcases correct rank
                await Testcases
                    .Where(t => t.Rank > testcase.Rank && t.ProblemId == testcase.ProblemId)
                    .BatchUpdateAsync(t => new Testcase { Rank = t.Rank - 1 });
                await tran.CommitAsync();
            }
            catch
            {
                dts = -1;
            }

            return dts;
        }

        async Task ITestcaseStore.ChangeRankAsync(int pid, int tid, bool up)
        {
            var tc = await Testcases
                .Where(t => t.ProblemId == pid && t.Id == tid)
                .FirstOrDefaultAsync();
            if (tc == null) return;

            int rk2 = tc.Rank + (up ? -1 : 1);
            var tc2 = await Testcases
                .Where(t => t.ProblemId == pid && t.Rank == rk2)
                .FirstOrDefaultAsync();

            if (tc2 != null)
            {
                var tcid1 = tc.Id;
                var tcid2 = tc2.Id;
                var rk1 = tc.Rank;
                await Testcases
                    .Where(t => t.Id == tcid1)
                    .BatchUpdateAsync(t => new Testcase { Rank = -1 });
                await Testcases
                    .Where(t => t.Id == tcid2)
                    .BatchUpdateAsync(t => new Testcase { Rank = rk1 });
                await Testcases
                    .Where(t => t.Id == tcid1)
                    .BatchUpdateAsync(t => new Testcase { Rank = rk2 });
            }
        }

        Task<int> ITestcaseStore.CountAsync(int pid)
        {
            return Testcases
                .Where(p => p.ProblemId == pid)
                .CountAsync();
        }

        async Task<(int, int)> ITestcaseStore.CountAndScoreAsync(int pid)
        {
            var q = await Testcases
                .Where(t => t.ProblemId == pid)
                .GroupBy(t => 1)
                .Select(g => new { Count = g.Count(), Score = g.Sum(t => t.Point) })
                .FirstOrDefaultAsync();
            return (q.Count, q.Score);
        }

        Task<Testcase> ITestcaseStore.CreateAsync(Testcase entity) => CreateEntityAsync(entity);

        Task<Testcase> ITestcaseStore.FindAsync(int tid, int? pid)
        {
            return Testcases
                .WhereIf(pid.HasValue, t => t.ProblemId == pid)
                .Where(t => t.Id == tid)
                .SingleOrDefaultAsync();
        }

        Task<IFileInfo> ITestcaseStore.GetFileAsync(Testcase tc, string target)
        {
            return ProblemFiles.GetFileInfoAsync($"p{tc.ProblemId}/t{tc.Id}.{target}");
        }

        Task<List<Testcase>> ITestcaseStore.ListAsync(int problemId, bool? secret)
        {
            return Testcases
                .Where(t => t.ProblemId == problemId)
                .WhereIf(secret.HasValue, t => t.IsSecret == secret)
                .OrderBy(t => t.Rank)
                .ToListAsync();
        }

        Task<IFileInfo> ITestcaseStore.SetFileAsync(Testcase tc, string target, Stream source)
        {
            return ProblemFiles.WriteStreamAsync($"p{tc.ProblemId}/t{tc.Id}.{target}", source);
        }

        Task ITestcaseStore.UpdateAsync(Testcase entity) => UpdateEntityAsync(entity);

        Task ITestcaseStore.UpdateAsync(int id, Expression<Func<Testcase, Testcase>> expression) => Testcases.Where(t => t.Id == id).BatchUpdateAsync(expression);
    }
}
