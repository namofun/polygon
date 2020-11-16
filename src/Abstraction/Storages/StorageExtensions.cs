﻿using Microsoft.Extensions.DependencyInjection;
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
    /// <summary>
    /// Extension methods for storage related class.
    /// </summary>
    public static class StorageExtensions
    {
        /// <summary>
        /// The available markdown files
        /// </summary>
        public static readonly string[] MarkdownFiles = new[]
        {
            "description",
            "inputdesc",
            "outputdesc",
            "hint",
            "interact"
        };

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
        /// <param name="fileName">The file name.</param>
        /// <returns>The task for fetching the file, resulting in the <see cref="IFileInfo"/>.</returns>
        public static Task<IFileInfo> GetFileAsync(this IProblemStore that, Problem problem, string fileName) => that.GetFileAsync(problem.Id, fileName);

        /// <summary>
        /// Find the judging with judging ID.
        /// </summary>
        /// <typeparam name="T">The data transfer object type.</typeparam>
        /// <param name="that">The store.</param>
        /// <param name="judgingId">The judging ID.</param>
        /// <param name="selector">The result selector.</param>
        /// <returns>The task for fetching corresponding DTO.</returns>
        public static Task<T> FindAsync<T>(this IJudgingStore that, int judgingId, Expression<Func<Judging, T>> selector)
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
        public static async Task<IEnumerable<(JudgingRun, Testcase)>> GetDetailsAsync(this IJudgingStore that, int problemId, int judgingId)
        {
            var result = await that.GetDetailsAsync(problemId, judgingId, (t, d) => new { t, d });
            return result.Select(a => (a.d, a.t));
        }

        /// <summary>
        /// Create an expression for fetching with <see cref="Submission"/> and <see cref="Judging"/>.
        /// </summary>
        /// <param name="includeDetails">Whether to include judging runs.</param>
        /// <returns>The expression for query.</returns>
        private static Expression<Func<Submission, Judging, Models.Solution>> CreateSelector(bool includeDetails)
        {
            if (includeDetails)
                return (s, j) => new Models.Solution
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
                    Details = j.Details,
                    TotalScore = j.TotalScore,
                };
            else
                return (s, j) => new Models.Solution
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
            Expression<Func<Submission, bool>>? predicate = null,
            bool includeDetails = false)
        {
            return that.ListWithJudgingAsync(pagination, CreateSelector(includeDetails), predicate);
        }

        /// <summary>
        /// List the solutions satisfying some conditions.
        /// </summary>
        /// <param name="that">The store.</param>
        /// <param name="predicate">The conditions.</param>
        /// <param name="includeDetails">Whether to include judging runs.</param>
        /// <param name="limits">The count to take.</param>
        /// <returns>The task for fetching solutions.</returns>
        public static Task<IEnumerable<Models.Solution>> ListWithJudgingAsync(
            this ISubmissionStore that,
            Expression<Func<Submission, bool>>? predicate = null,
            bool includeDetails = false,
            int? limits = null)
        {
            return that.ListWithJudgingAsync(CreateSelector(includeDetails), predicate, limits);
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
        /// <param name="input">The input file.</param>
        /// <param name="output">The output file.</param>
        /// <returns>The created entity.</returns>
        public static async Task<Testcase> CreateAsync(this ITestcaseStore store, Testcase entity, Stream input, Stream output)
        {
            if (!input.CanSeek || !output.CanSeek)
            {
                throw new InvalidOperationException("The type of input and output is not correct.");
            }

            input.Seek(0, SeekOrigin.Begin);
            output.Seek(0, SeekOrigin.End);
            entity.Md5sumInput = input.ToMD5().ToHexDigest(true);
            entity.Md5sumOutput = output.ToMD5().ToHexDigest(true);
            entity.InputLength = (int)input.Length;
            entity.OutputLength = (int)output.Length;

            await store.CreateAsync(entity);

            input.Seek(0, SeekOrigin.Begin);
            output.Seek(0, SeekOrigin.End);
            await store.SetFileAsync(entity, "in", input);
            await store.SetFileAsync(entity, "out", output);

            return entity;
        }

        /// <summary>
        /// Get the testcase input file.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="testcase">The testcase.</param>
        /// <returns>The task for fetching the file, resulting in the <see cref="IFileInfo"/>.</returns>
        public static Task<IFileInfo> GetInputAsync(this ITestcaseStore store, Testcase testcase)
        {
            return store.GetFileAsync(testcase, "in");
        }

        /// <summary>
        /// Get the testcase output file.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="testcase">The testcase.</param>
        /// <returns>The task for fetching the file, resulting in the <see cref="IFileInfo"/>.</returns>
        public static Task<IFileInfo> GetOutputAsync(this ITestcaseStore store, Testcase testcase)
        {
            return store.GetFileAsync(testcase, "out");
        }

        /// <summary>
        /// Set the testcase input file.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="testcase">The testcase.</param>
        /// <param name="source">The source.</param>
        /// <returns>The task for fetching the file, resulting in the <see cref="IFileInfo"/>.</returns>
        public static async Task<IFileInfo> SetInputAsync(this ITestcaseStore store, Testcase testcase, Stream source)
        {
            if (!source.CanSeek)
            {
                throw new InvalidOperationException("The type of file is not correct.");
            }

            source.Seek(0, SeekOrigin.Begin);
            testcase.InputLength = (int)source.Length;
            testcase.Md5sumInput = source.ToMD5().ToHexDigest(true);
            await store.UpdateAsync(testcase);
            source.Seek(0, SeekOrigin.Begin);
            return await store.SetFileAsync(testcase, "in", source);
        }

        /// <summary>
        /// Set the testcase output file.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="testcase">The testcase.</param>
        /// <param name="source">The source.</param>
        /// <returns>The task for fetching the file, resulting in the <see cref="IFileInfo"/>.</returns>
        public static async Task<IFileInfo> SetOutputAsync(this ITestcaseStore store, Testcase testcase, Stream source)
        {
            if (!source.CanSeek)
            {
                throw new InvalidOperationException("The type of file is not correct.");
            }

            source.Seek(0, SeekOrigin.Begin);
            testcase.OutputLength = (int)source.Length;
            testcase.Md5sumOutput = source.ToMD5().ToHexDigest(true);
            await store.UpdateAsync(testcase);
            source.Seek(0, SeekOrigin.Begin);
            return await store.SetFileAsync(testcase, "out", source);
        }

        /// <summary>
        /// Add polygon facade implemention.
        /// </summary>
        /// <typeparam name="TFacade">The facade implemention type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddPolygonStorage<TFacade>(this IServiceCollection services) where TFacade : class, IPolygonFacade2
        {
            return services
                .AddScoped<IPolygonFacade2, TFacade>()
                .AddScoped<IPolygonFacade>(s => s.GetRequiredService<IPolygonFacade2>())
                .AddScoped<IExecutableStore>(s => s.GetRequiredService<IPolygonFacade2>())
                .AddScoped<IInternalErrorStore>(s => s.GetRequiredService<IPolygonFacade2>())
                .AddScoped<IJudgehostStore>(s => s.GetRequiredService<IPolygonFacade2>())
                .AddScoped<IJudgingStore>(s => s.GetRequiredService<IPolygonFacade2>())
                .AddScoped<ILanguageStore>(s => s.GetRequiredService<IPolygonFacade2>())
                .AddScoped<IProblemStore>(s => s.GetRequiredService<IPolygonFacade2>())
                .AddScoped<IRejudgingStore>(s => s.GetRequiredService<IPolygonFacade2>())
                .AddScoped<ISubmissionStore>(s => s.GetRequiredService<IPolygonFacade2>())
                .AddScoped<ITestcaseStore>(s => s.GetRequiredService<IPolygonFacade2>());
        }

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
