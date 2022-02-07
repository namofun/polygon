using Microsoft.Extensions.FileProviders;
using Polygon.Entities;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    /// <summary>
    /// The store interface for <see cref="Judging"/>.
    /// </summary>
    public interface IJudgingStore
    {
        /// <summary>
        /// Create an instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The created entity.</returns>
        Task<Judging> CreateAsync(Judging entity);

        /// <summary>
        /// Create an instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The created entity.</returns>
        Task<JudgingRun> InsertAsync(JudgingRun entity);

        /// <summary>
        /// Update the instance of entity.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <param name="expression">The update expression.</param>
        /// <returns>The update task.</returns>
        Task UpdateAsync(int id, Expression<Func<Judging, Judging>> expression);

        /// <summary>
        /// Count the judgings with predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The count task.</returns>
        Task<int> CountAsync(Expression<Func<Judging, bool>> predicate);

        /// <summary>
        /// Make a summary for such judging.
        /// </summary>
        /// <param name="judgingId">The judging ID.</param>
        /// <returns>The summary. May be null when no previous runnings.</returns>
        Task<RunSummary> SummarizeAsync(int judgingId);

        /// <summary>
        /// Get the status of judge queue.
        /// </summary>
        /// <param name="cid">The contest ID. <c>null</c> for all.</param>
        /// <returns>The list of judge queues.</returns>
        Task<List<ServerStatus>> GetJudgeQueueAsync(int? cid = null);

        /// <summary>
        /// Fetch the run result file of judging run.
        /// </summary>
        /// <param name="judgingId">The judging ID.</param>
        /// <param name="runId">The run ID.</param>
        /// <param name="type">The file type.</param>
        /// <param name="submissionId">The submission ID. Just for validation.</param>
        /// <param name="problemId">The problem ID. Just for validation.</param>
        /// <returns>The fetched file info.</returns>
        Task<IBlobInfo> GetRunFileAsync(int judgingId, int runId, string type, int? submissionId = null, int? problemId = null);

        /// <summary>
        /// Save the run result file of judging run.
        /// </summary>
        /// <param name="judgingId">The judging ID.</param>
        /// <param name="runId">The run ID.</param>
        /// <param name="type">The file type.</param>
        /// <param name="content">The file content.</param>
        /// <returns>The saved file info.</returns>
        Task<IBlobInfo> SetRunFileAsync(int judgingId, int runId, string type, byte[] content);

        /// <summary>
        /// Find the judging with predicate.
        /// </summary>
        /// <typeparam name="T">The result DTO type.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The task for fetching DTO.</returns>
        Task<T?> FindAsync<T>(Expression<Func<Judging, bool>> predicate, Expression<Func<Judging, T>> selector) where T : class;

        /// <summary>
        /// List the judgings with predicate and selector.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="topCount">The top count.</param>
        /// <returns>The list task.</returns>
        Task<List<T>> ListAsync<T>(Expression<Func<Judging, bool>> predicate, Expression<Func<Judging, T>> selector, int topCount);

        /// <summary>
        /// List the judgings with predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="topCount">The top count.</param>
        /// <returns>The list task.</returns>
        Task<List<Judging>> ListAsync(Expression<Func<Judging, bool>> predicate, int topCount);

        /// <summary>
        /// Fetch the detail DTO.
        /// </summary>
        /// <param name="problemId">The problem ID.</param>
        /// <param name="submitId">The submission ID.</param>
        /// <param name="judgingId">The judging ID.</param>
        /// <param name="runId">The judging run ID.</param>
        /// <returns>The task for fetching judging run.</returns>
        Task<JudgingRun?> GetDetailAsync(int problemId, int submitId, int judgingId, int runId);

        /// <summary>
        /// Fetches the judging run entities.
        /// </summary>
        /// <param name="judgingIds">The judging IDs that judging runs belongs to.</param>
        /// <returns>The task for getting the lookup.</returns>
        Task<ILookup<int, JudgingRun>> GetJudgingRunsAsync(IEnumerable<int> judgingIds);

        /// <summary>
        /// Fetch the details DTO.
        /// </summary>
        /// <remarks>Use left join so the judging run may be null.</remarks>
        /// <typeparam name="T">The result DTO.</typeparam>
        /// <param name="problemId">The problem ID.</param>
        /// <param name="judgingId">The judging ID.</param>
        /// <param name="selector">The result selector.</param>
        /// <returns>The task for fetching judging runs.</returns>
        Task<IEnumerable<T>> GetDetailsAsync<T>(int problemId, int judgingId, Expression<Func<Testcase, JudgingRun?, T>> selector) where T : class;

        /// <summary>
        /// Fetch the details DTO.
        /// </summary>
        /// <remarks>Use inner join so the judging run is not null.</remarks>
        /// <typeparam name="T">The result DTO.</typeparam>
        /// <param name="selector">The result selector.</param>
        /// <param name="predicate">The condition.</param>
        /// <param name="limit">The count of selects.</param>
        /// <returns>The task for fetching judging runs.</returns>
        Task<IEnumerable<T>> GetDetailsAsync<T>(Expression<Func<Testcase, JudgingRun, T>> selector, Expression<Func<Testcase, JudgingRun, bool>>? predicate = null, int? limit = null);
    }
}
