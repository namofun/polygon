﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon.Storages
{
    /// <summary>
    /// Extension methods for storage related class.
    /// </summary>
    public static class StorageExtensions
    {
        /// <summary>
        /// Finds the problem via the claims principal.
        /// </summary>
        /// <param name="store">The problem store.</param>
        /// <param name="problemId">The problem ID.</param>
        /// <param name="user">The user claims principal.</param>
        /// <returns>The task for fetching the problem and author level.</returns>
        public static async Task<(Problem?, AuthorLevel?)> FindAsync(this IProblemStore store, int problemId, ClaimsPrincipal user)
        {
            if (user.IsInRole("Administrator"))
            {
                var prob = await store.FindAsync(problemId);
                return (prob, prob == null ? default(AuthorLevel?) : AuthorLevel.Creator);
            }
            else if (int.TryParse(user.GetUserId(), out int uid))
            {
                return await store.FindAsync(problemId, uid);
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Count the testcases.
        /// </summary>
        /// <param name="that">The store.</param>
        /// <param name="problem">The problem.</param>
        /// <returns>The count task.</returns>
        public static Task<int> CountAsync(this ITestcaseStore that, Problem problem) => that.CountAsync(problem.Id);

        /// <summary>
        /// Get the testcase file.
        /// </summary>
        /// <param name="that">The store.</param>
        /// <param name="problem">The problem.</param>
        /// <param name="sectionName">The section name.</param>
        /// <returns>The task for fetching the file, resulting in the <see cref="IBlobInfo"/>.</returns>
        public static Task<IBlobInfo> GetStatementSectionAsync(this IProblemStore that, Problem problem, string sectionName) => that.GetStatementSectionAsync(problem.Id, sectionName);

        /// <summary>
        /// Find the judging with judging ID.
        /// </summary>
        /// <typeparam name="T">The data transfer object type.</typeparam>
        /// <param name="that">The store.</param>
        /// <param name="judgingId">The judging ID.</param>
        /// <param name="selector">The result selector.</param>
        /// <returns>The task for fetching corresponding DTO.</returns>
        public static Task<T?> FindAsync<T>(this IJudgingStore that, int judgingId, Expression<Func<Judging, T>> selector) where T : class
        {
            return that.FindAsync(j => j.Id == judgingId, selector);
        }

        /// <summary>
        /// Find the judging with judging ID.
        /// </summary>
        /// <param name="that">The store.</param>
        /// <param name="judgingId">The judging ID.</param>
        /// <returns>The task for fetching corresponding DTO.</returns>
        public static async Task<(Judging judging, int problemId, int contestId, int teamId, DateTimeOffset time)> FindAsync(this IJudgingStore that, int judgingId)
        {
            var result = await that.FindAsync(
                j => j.Id == judgingId,
                j => new { j, j.s.ProblemId, j.s.ContestId, j.s.TeamId, j.s.Time, });
            if (result == null) return default;
            return (result.j, result.ProblemId, result.ContestId, result.TeamId, result.Time);
        }

        /// <summary>
        /// Fetch the details with not-used testcases.
        /// </summary>
        /// <param name="that">The store.</param>
        /// <param name="problemId">The problem ID.</param>
        /// <param name="judgingId">The judging ID.</param>
        /// <returns>The task for fetching pairs.</returns>
        public static async Task<IEnumerable<(JudgingRun?, Testcase)>> GetDetailsAsync(this IJudgingStore that, int problemId, int judgingId)
        {
            var result = await that.GetDetailsAsync(problemId, judgingId, (t, d) => new { t, d });
            return result.Select(a => ((JudgingRun?)a.d, a.t));
        }

        private static readonly Expression<Func<Submission, Judging, Models.Solution>> SelectorWithDetails
            = (s, j) => new Models.SolutionV1
            {
                SubmissionId = s.Id,
                JudgingId = j.Id,
                ProblemId = s.ProblemId,
                ContestId = s.ContestId,
                TeamId = s.TeamId,
                Skipped = s.Ignored,
                Language = s.Language,
                CodeLength = s.CodeLength,
                ExpectedVerdict = s.ExpectedResult,
                Time = s.Time,
                Ip = s.Ip,
                Verdict = j.Status,
                ExecutionTime = j.ExecuteTime,
                ExecutionMemory = j.ExecuteMemory,
                RunVerdictsRaw = j.RunVerdicts,
                TotalScore = j.TotalScore,
            };

        private static readonly Expression<Func<Submission, Judging, Models.Solution>> SelectorWithoutDetails
            = (s, j) => new Models.Solution
            {
                SubmissionId = s.Id,
                JudgingId = j.Id,
                ProblemId = s.ProblemId,
                ContestId = s.ContestId,
                TeamId = s.TeamId,
                Skipped = s.Ignored,
                Language = s.Language,
                CodeLength = s.CodeLength,
                ExpectedVerdict = s.ExpectedResult,
                Time = s.Time,
                Ip = s.Ip,
                Verdict = j.Status,
                ExecutionTime = j.ExecuteTime,
                ExecutionMemory = j.ExecuteMemory,
                TotalScore = j.TotalScore,
            };

        /// <summary>
        /// List the paginated solutions satisfying some conditions.
        /// </summary>
        /// <param name="that">The store.</param>
        /// <param name="pagination">The pagination parameter.</param>
        /// <param name="predicate">The conditions.</param>
        /// <param name="includeDetails">Whether to include judging runs.</param>
        /// <returns>The task for fetching solutions.</returns>
        public static Task<IPagedList<Models.Solution>> ListWithJudgingAsync(
            this ISubmissionStore that,
            (int Page, int PageCount) pagination,
            Expression<Func<Submission, bool>>? predicate = null,
            bool includeDetails = false)
        {
            return that.ListWithJudgingAsync(pagination, includeDetails ? SelectorWithDetails : SelectorWithoutDetails, predicate);
        }

        /// <summary>
        /// List the paginated solutions satisfying some conditions.
        /// </summary>
        /// <param name="that">The store.</param>
        /// <param name="pagination">The pagination parameter.</param>
        /// <param name="predicate">The conditions.</param>
        /// <param name="includeDetails">Whether to include judging runs.</param>
        /// <returns>The task for fetching solutions.</returns>
        public static Task<IPagedList<Models.Solution>> ListWithJudgingAsync(
            this ISubmissionStore that,
            (int Page, int PageCount) pagination,
            Expression<Func<Submission, Judging, bool>> predicate,
            bool includeDetails = false)
        {
            return that.ListWithJudgingAsync(pagination, includeDetails ? SelectorWithDetails : SelectorWithoutDetails, predicate);
        }

        /// <summary>
        /// List the solutions satisfying some conditions.
        /// </summary>
        /// <param name="that">The store.</param>
        /// <param name="predicate">The conditions.</param>
        /// <param name="includeDetails">Whether to include judging runs.</param>
        /// <param name="limits">The count to take.</param>
        /// <returns>The task for fetching solutions.</returns>
        public static Task<List<Models.Solution>> ListWithJudgingAsync(
            this ISubmissionStore that,
            Expression<Func<Submission, bool>>? predicate = null,
            bool includeDetails = false,
            int? limits = null)
        {
            return that.ListWithJudgingAsync(includeDetails ? SelectorWithDetails : SelectorWithoutDetails, predicate, limits);
        }

        /// <summary>
        /// Get the author name of submission.
        /// </summary>
        /// <param name="that">The store.</param>
        /// <param name="submissionId">The submission ID.</param>
        /// <returns>The author name.</returns>
        public static async Task<string> GetAuthorNameAsync(this ISubmissionStore that, int submissionId)
        {
            var result = await that.GetAuthorNamesAsync(s => s.Id == submissionId);
            return result.SingleOrDefault().Value;
        }

        /// <summary>
        /// Create an instance of entity.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="inputFactory">The stream factory for input file.</param>
        /// <param name="inputLength">The input file length.</param>
        /// <param name="outputFactory">The stream factory for output file.</param>
        /// <param name="outputLength">The output file length.</param>
        /// <returns>The created entity.</returns>
        public static async Task<Testcase> CreateAsync(
            this ITestcaseStore store,
            Testcase entity,
            Func<Stream> inputFactory, long inputLength,
            Func<Stream> outputFactory, long outputLength)
        {
            using var input = await (inputFactory, inputLength).CreateAsync();
            using var output = await (outputFactory, outputLength).CreateAsync();

            entity.Md5sumInput = await input.Md5HashAsync();
            entity.Md5sumOutput = await output.Md5HashAsync();
            entity.InputLength = (int)input.Length;
            entity.OutputLength = (int)output.Length;
            entity.Rank = 1 + await store.CountAsync(entity.ProblemId);

            await store.CreateAsync(entity);

            using (var inputs = input.OpenRead())
                await store.SetFileAsync(entity, "in", inputs);
            using (var outputs = output.OpenRead())
                await store.SetFileAsync(entity, "out", outputs);

            return entity;
        }

        /// <summary>
        /// Get the testcase input file.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="testcase">The testcase.</param>
        /// <returns>The task for fetching the file, resulting in the <see cref="IBlobInfo"/>.</returns>
        public static Task<IBlobInfo> GetInputAsync(this ITestcaseStore store, Testcase testcase)
        {
            return store.GetFileAsync(testcase, "in");
        }

        /// <summary>
        /// Get the testcase output file.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="testcase">The testcase.</param>
        /// <returns>The task for fetching the file, resulting in the <see cref="IBlobInfo"/>.</returns>
        public static Task<IBlobInfo> GetOutputAsync(this ITestcaseStore store, Testcase testcase)
        {
            return store.GetFileAsync(testcase, "out");
        }

        /// <summary>
        /// List available problems.
        /// </summary>
        /// <param name="store">The problem store.</param>
        /// <param name="page">The page.</param>
        /// <param name="perCount">The count per page.</param>
        /// <param name="user">The user claims principal.</param>
        /// <param name="ascending">Whether to pagination as ascending.</param>
        /// <param name="leastLevel">The least problem level to show.</param>
        /// <returns>The task for paginated list of problems.</returns>
        public static Task<IPagedList<Problem>> ListAsync(this IProblemStore store, int page, int perCount, ClaimsPrincipal user, bool ascending = true, AuthorLevel? leastLevel = null)
        {
            var uid = user.IsInRole("Administrator")
                ? default(int?)
                : int.Parse(user.GetUserId() ?? "-100");

            return store.ListAsync(page, perCount, ascending, uid, leastLevel);
        }

        /// <summary>
        /// Read the content of file as string.
        /// </summary>
        /// <param name="file">The blob info.</param>
        /// <returns>The task for reading content. <c>Null</c> if the file does not exist.</returns>
        public static async Task<string?> ReadAsStringAsync(this IBlobInfo file)
        {
            if (!file.Exists) return null;
            using var fs = await file.CreateReadStreamAsync();
            using var sw = new StreamReader(fs);
            return await sw.ReadToEndAsync();
        }

        /// <summary>
        /// Read the content of file as string.
        /// </summary>
        /// <param name="file">The blob info.</param>
        /// <returns>The task for reading content. <c>Null</c> if the file does not exist.</returns>
        public static async Task<string?> ReadAsStringAndCacheAsync(this IBlobInfo file)
        {
            if (!file.Exists) return null;
            using var fs = await file.CreateReadStreamAsync(true);
            using var sw = new StreamReader(fs);
            return await sw.ReadToEndAsync();
        }

        /// <summary>
        /// Set the testcase output file.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="testcase">The testcase.</param>
        /// <param name="inputf">The input file.</param>
        /// <param name="outputf">The output file.</param>
        /// <returns>The task for updating testcase.</returns>
        public static async Task UpdateAsync(
            this ITestcaseStore store,
            Testcase testcase,
            (Func<Stream>, long)? inputf,
            (Func<Stream>, long)? outputf)
        {
            IStream2? input = null, output = null;

            if (inputf != null)
            {
                input = await inputf.Value.CreateAsync();
                testcase.Md5sumInput = await input.Md5HashAsync();
                testcase.InputLength = (int)input.Length;
            }

            if (outputf != null)
            {
                output = await outputf.Value.CreateAsync();
                testcase.Md5sumOutput = await output.Md5HashAsync();
                testcase.OutputLength = (int)output.Length;
            }

            await store.UpdateAsync(testcase);

            if (input != null)
            {
                using (var inputs = input.OpenRead())
                    await store.SetFileAsync(testcase, "in", inputs);
                input.Dispose();
            }

            if (output != null)
            {
                using (var outputs = output.OpenRead())
                    await store.SetFileAsync(testcase, "out", outputs);
                output.Dispose();
            }
        }

        /// <summary>
        /// Add polygon facade implemention.
        /// </summary>
        /// <typeparam name="TFacade">The facade implemention type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddPolygonStorage<TFacade>(this IServiceCollection services)
            where TFacade : class, IPolygonFacade,
                            IProblemStore, ITestcaseStore, ISubmissionStore,
                            IExecutableStore, IInternalErrorStore, IJudgehostStore,
                            IJudgingStore, ILanguageStore, IRejudgingStore
        {
            return services
                .AddScoped<TFacade>()
                .AddScoped<IPolygonFacade>(s => s.GetRequiredService<TFacade>())
                .AddScoped<IExecutableStore>(s => s.GetRequiredService<TFacade>())
                .AddScoped<IInternalErrorStore>(s => s.GetRequiredService<TFacade>())
                .AddScoped<IJudgehostStore>(s => s.GetRequiredService<TFacade>())
                .AddScoped<IJudgingStore>(s => s.GetRequiredService<TFacade>())
                .AddScoped<ILanguageStore>(s => s.GetRequiredService<TFacade>())
                .AddScoped<IProblemStore>(s => s.GetRequiredService<TFacade>())
                .AddScoped<IRejudgingStore>(s => s.GetRequiredService<TFacade>())
                .AddScoped<ISubmissionStore>(s => s.GetRequiredService<TFacade>())
                .AddScoped<ITestcaseStore>(s => s.GetRequiredService<TFacade>());
        }

        /// <summary>
        /// Add polygon facade implemention.
        /// </summary>
        /// <typeparam name="TExecutableStore">The executable store implemention type.</typeparam>
        /// <typeparam name="TInternalErrorStore">The internal error store implemention type.</typeparam>
        /// <typeparam name="TJudgehostStore">The judgehost store implemention type.</typeparam>
        /// <typeparam name="TJudgingStore">The judging store implemention type.</typeparam>
        /// <typeparam name="TLanguageStore">The language store implemention type.</typeparam>
        /// <typeparam name="TProblemStore">The problem store implemention type.</typeparam>
        /// <typeparam name="TRejudgingStore">The rejudging store implemention type.</typeparam>
        /// <typeparam name="TSubmissionStore">The submission store implemention type.</typeparam>
        /// <typeparam name="TTestcaseStore">The testcase store implemention type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddPolygonStorage<
            TExecutableStore, TInternalErrorStore, TJudgehostStore,
            TJudgingStore, TLanguageStore, TProblemStore,
            TRejudgingStore, TSubmissionStore, TTestcaseStore>(
            this IServiceCollection services)
            where TExecutableStore : class, IExecutableStore
            where TInternalErrorStore : class, IInternalErrorStore
            where TJudgehostStore : class, IJudgehostStore
            where TJudgingStore : class, IJudgingStore
            where TLanguageStore : class, ILanguageStore
            where TProblemStore : class, IProblemStore
            where TRejudgingStore : class, IRejudgingStore
            where TSubmissionStore : class, ISubmissionStore
            where TTestcaseStore : class, ITestcaseStore
        {
            return services
                .AddScoped<IPolygonFacade, CompositePolygonFacade>()
                .AddScoped<IExecutableStore, TExecutableStore>()
                .AddScoped<IInternalErrorStore, TInternalErrorStore>()
                .AddScoped<IJudgehostStore, TJudgehostStore>()
                .AddScoped<IJudgingStore, TJudgingStore>()
                .AddScoped<ILanguageStore, TLanguageStore>()
                .AddScoped<IProblemStore, TProblemStore>()
                .AddScoped<IRejudgingStore, TRejudgingStore>()
                .AddScoped<ISubmissionStore, TSubmissionStore>()
                .AddScoped<ITestcaseStore, TTestcaseStore>();
        }
    }
}
