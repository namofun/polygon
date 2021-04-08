using Polygon.Entities;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    /// <summary>
    /// The store interface for <see cref="Submission"/>.
    /// </summary>
    public interface ISubmissionStore
    {
        /// <summary>
        /// Create a submission with parameters.
        /// </summary>
        /// <param name="code">The source code without base64.</param>
        /// <param name="language">The language ID.</param>
        /// <param name="problemId">The problem ID.</param>
        /// <param name="contestId">The contest ID.</param>
        /// <param name="teamId">The team ID.</param>
        /// <param name="ipAddr">The IP Address.</param>
        /// <param name="via">The submission source or tool.</param>
        /// <param name="username">The username who submitted this solution.</param>
        /// <param name="expected">The expected verdict.</param>
        /// <param name="time">The submission time.</param>
        /// <param name="fullJudge">Whether to full judge this solution.</param>
        /// <returns>The task for creating submission entity.</returns>
        Task<Submission> CreateAsync(
            string code,
            string language,
            int problemId,
            int? contestId,
            int teamId,
            IPAddress ipAddr,
            string via,
            string username,
            Verdict? expected = null,
            DateTimeOffset? time = null,
            bool fullJudge = false);

        /// <summary>
        /// Update the instance of entity.
        /// </summary>
        /// <param name="submission">The submission entity.</param>
        /// <param name="expression">The update expression.</param>
        /// <returns>The update task.</returns>
        Task UpdateAsync(Submission submission, Expression<Func<Submission, Submission>> expression);

        /// <summary>
        /// Find the submission.
        /// </summary>
        /// <param name="submissionId">The submission ID.</param>
        /// <param name="includeJudgings">Whether to include judgings in results.</param>
        /// <returns>The task for fetching submission entity.</returns>
        Task<Submission> FindAsync(int submissionId, bool includeJudgings = false);

        /// <summary>
        /// Find the submission by judging ID.
        /// </summary>
        /// <param name="judgingid">The judging ID.</param>
        /// <returns>The submission entity.</returns>
        Task<Submission> FindByJudgingAsync(int judgingid);

        /// <summary>
        /// Batch get the author name of submissions.
        /// </summary>
        /// <param name="submitids">The submission ID filter.</param>
        /// <returns>The dictionary for author names.</returns>
        Task<Dictionary<int, string>> GetAuthorNamesAsync(Expression<Func<Submission, bool>> submitids);

        /// <summary>
        /// List the submissions.
        /// </summary>
        /// <typeparam name="T">The DTO entity.</typeparam>
        /// <param name="projection">The entity shaper.</param>
        /// <param name="predicate">The submission filter.</param>
        /// <param name="limit">The submission count to take.</param>
        /// <returns>The submission list.</returns>
        Task<List<T>> ListAsync<T>(
            Expression<Func<Submission, T>> projection,
            Expression<Func<Submission, bool>>? predicate = null,
            int? limit = null);

        /// <summary>
        /// List the paginated solutions satisfying some conditions.
        /// </summary>
        /// <typeparam name="T">The DTO entity.</typeparam>
        /// <param name="pagination">The pagination parameter.</param>
        /// <param name="selector">The entity shaper.</param>
        /// <param name="predicate">The conditions.</param>
        /// <returns>The task for fetching solutions.</returns>
        Task<IPagedList<T>> ListWithJudgingAsync<T>(
            (int Page, int PageCount) pagination,
            Expression<Func<Submission, Judging, T>> selector,
            Expression<Func<Submission, bool>>? predicate = null);

        /// <summary>
        /// List the solutions satisfying some conditions.
        /// </summary>
        /// <typeparam name="T">The DTO entity.</typeparam>
        /// <param name="selector">The entity shaper.</param>
        /// <param name="predicate">The conditions.</param>
        /// <param name="limits">The count to take.</param>
        /// <returns>The task for fetching solutions.</returns>
        Task<List<T>> ListWithJudgingAsync<T>(
            Expression<Func<Submission, Judging, T>> selector,
            Expression<Func<Submission, bool>>? predicate = null,
            int? limits = null);

        /// <summary>
        /// Update the statistics for some participant and problem.
        /// </summary>
        /// <param name="contestId">The contest ID.</param>
        /// <param name="teamId">The team ID.</param>
        /// <param name="problemId">The problem ID.</param>
        /// <param name="accepted">Whether accepted.</param>
        /// <returns></returns>
        Task UpdateStatisticsAsync(int contestId, int teamId, int problemId, bool accepted);

        /// <summary>
        /// Get the submission file.
        /// </summary>
        /// <param name="submissionId">The submission ID.</param>
        /// <returns>The task for fetching submission file.</returns>
        Task<SubmissionFile> GetFileAsync(int submissionId);

        /// <summary>
        /// Statistics the submission for participant.
        /// </summary>
        /// <param name="contestId">The contest ID.</param>
        /// <param name="teamId">The team ID.</param>
        /// <returns>The task for fetching cached statistics results.</returns>
        Task<List<SubmissionStatistics>> StatisticsAsync(int contestId, int teamId);
    }
}
