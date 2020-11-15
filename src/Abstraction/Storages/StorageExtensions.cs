using Microsoft.Extensions.FileProviders;
using Polygon.Entities;
using System;
using System.Collections.Generic;
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
    }
}
