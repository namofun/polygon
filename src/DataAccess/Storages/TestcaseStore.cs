﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Models;

namespace Xylab.Polygon.Storages
{
    public partial class PolygonFacade<TContext, TQueryCache> : ITestcaseStore
    {
        Task<int> ITestcaseStore.BatchScoreAsync(int probid, int lower, int upper, int score)
        {
            return Context.Testcases
                .Where(t => t.ProblemId == probid && t.Rank >= lower && t.Rank <= upper)
                .BatchUpdateAsync(t => new Testcase { Point = score });
        }

        async Task<TestcaseDeleteResult> ITestcaseStore.CascadeDeleteAsync(Testcase testcase)
        {
            if (await Context.Judgings
                .Where(j => j.s.ProblemId == testcase.ProblemId && j.Status == Verdict.Running)
                .AnyAsync())
                return TestcaseDeleteResult.Fail(
                    "Some judging tasks are running for this problem, cannot delete now.");

            await using var tran = await Context.Database.BeginTransactionAsync();

            try
            {
                // details are set ON DELETE NO ACTION, so we have to delete it before
                int dts = await Context.JudgingRuns
                    .Where(d => d.TestcaseId == testcase.Id)
                    .BatchDeleteAsync();

                await Context.Testcases
                    .Where(t => t.Id == testcase.Id)
                    .BatchDeleteAsync();

                // set the rest testcases correct rank
                await Context.Testcases
                    .Where(t => t.Rank > testcase.Rank && t.ProblemId == testcase.ProblemId)
                    .BatchUpdateAsync(t => new Testcase { Rank = t.Rank - 1 });

                await tran.CommitAsync();

                return TestcaseDeleteResult.Succeed(dts);
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();

                return TestcaseDeleteResult.Fail(ex.Message);
            }
        }

        async Task ITestcaseStore.ChangeRankAsync(int probid, int testid, bool up)
        {
            var tc = await Context.Testcases
                .Where(t => t.ProblemId == probid && t.Id == testid)
                .FirstOrDefaultAsync();
            if (tc == null) return;

            int rk2 = tc.Rank + (up ? -1 : 1);
            var tc2 = await Context.Testcases
                .Where(t => t.ProblemId == probid && t.Rank == rk2)
                .FirstOrDefaultAsync();

            if (tc2 != null)
            {
                var tcid1 = tc.Id;
                var tcid2 = tc2.Id;
                var rk1 = tc.Rank;
                // swap the testcase

                await Context.Testcases
                    .Where(t => t.Id == tcid1)
                    .BatchUpdateAsync(t => new Testcase { Rank = -1 });

                await Context.Testcases
                    .Where(t => t.Id == tcid2)
                    .BatchUpdateAsync(t => new Testcase { Rank = rk1 });

                await Context.Testcases
                    .Where(t => t.Id == tcid1)
                    .BatchUpdateAsync(t => new Testcase { Rank = rk2 });
            }
        }

        Task<int> ITestcaseStore.CountAsync(int probid)
        {
            return Context.Testcases
                .Where(p => p.ProblemId == probid)
                .CountAsync();
        }

        async Task<(int, int)> ITestcaseStore.CountAndScoreAsync(int probid)
        {
            var q = await Context.Testcases
                .Where(t => t.ProblemId == probid)
                .GroupBy(t => 1)
                .Select(g => new { Count = g.Count(), Score = g.Sum(t => t.Point) })
                .FirstOrDefaultAsync();

            return (q?.Count ?? 0, q?.Score ?? 0);
        }

        Task<Testcase> ITestcaseStore.CreateAsync(Testcase entity) => CreateEntityAsync(entity);

        Task<Testcase?> ITestcaseStore.FindAsync(int testid, int? probid)
        {
            return Context.Testcases
                .WhereIf(probid.HasValue, t => t.ProblemId == probid)
                .Where(t => t.Id == testid)
                .SingleOrDefaultAsync();
        }

        Task<IBlobInfo> ITestcaseStore.GetFileAsync(Testcase tc, string target)
        {
            return ProblemFiles.GetTestcaseFileAsync(tc.ProblemId, tc.Id, target);
        }

        Task<List<Testcase>> ITestcaseStore.ListAsync(int problemId, bool? secret)
        {
            return Context.Testcases
                .Where(t => t.ProblemId == problemId)
                .WhereIf(secret.HasValue, t => t.IsSecret == secret)
                .OrderBy(t => t.Rank)
                .ToListAsync();
        }

        Task<IBlobInfo> ITestcaseStore.SetFileAsync(Testcase tc, string target, Stream source)
        {
            return ProblemFiles.WriteTestcaseFileAsync(tc.ProblemId, tc.Id, target, source);
        }

        Task ITestcaseStore.UpdateAsync(Testcase entity) => UpdateEntityAsync(entity);

        Task ITestcaseStore.UpdateAsync(int id, Expression<Func<Testcase, Testcase>> expression)
        {
            return Context.Testcases.Where(t => t.Id == id).BatchUpdateAsync(expression);
        }
    }
}
