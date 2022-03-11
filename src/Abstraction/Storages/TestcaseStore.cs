using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon.Storages
{
    /// <summary>
    /// The store interface for <see cref="Testcase"/>.
    /// </summary>
    public interface ITestcaseStore
    {
        /// <summary>
        /// Create an instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The created entity.</returns>
        Task<Testcase> CreateAsync(Testcase entity);

        /// <summary>
        /// Update the instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The update task.</returns>
        Task UpdateAsync(Testcase entity);

        /// <summary>
        /// Update the instance of entity.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <param name="expression">The update expression.</param>
        /// <returns>The update task.</returns>
        Task UpdateAsync(int id, Expression<Func<Testcase, Testcase>> expression);

        /// <summary>
        /// List the testcase from the problem.
        /// </summary>
        /// <param name="problemId">The problem ID.</param>
        /// <param name="secret">Whether to list only secret.</param>
        /// <returns>The testcase list.</returns>
        Task<List<Testcase>> ListAsync(int problemId, bool? secret = null);
        
        /// <summary>
        /// Find the testcase.
        /// </summary>
        /// <param name="testid">The testcase ID.</param>
        /// <param name="probid">The problem ID.</param>
        /// <returns>The find task.</returns>
        Task<Testcase?> FindAsync(int testid, int? probid = null);

        /// <summary>
        /// Batch set the score of testcases.
        /// </summary>
        /// <param name="probid">The problem ID.</param>
        /// <param name="lower">The lowest rank of testcase.</param>
        /// <param name="upper">The highest rank of testcase.</param>
        /// <param name="score">The score for each testcase.</param>
        /// <returns>The task for updated testcases.</returns>
        Task<int> BatchScoreAsync(int probid, int lower, int upper, int score);

        /// <summary>
        /// Cascade delete the testcase with details.
        /// </summary>
        /// <param name="testcase">The testcase to delete.</param>
        /// <returns>The task for delete.</returns>
        Task<int> CascadeDeleteAsync(Testcase testcase);

        /// <summary>
        /// Change the rank of testcase.
        /// </summary>
        /// <param name="problemId">The problem ID.</param>
        /// <param name="testcaseId">The testcase ID.</param>
        /// <param name="up">The direction (<c>true</c> for up, <c>false</c> for down).</param>
        /// <returns>The task for changing rank.</returns>
        Task ChangeRankAsync(int problemId, int testcaseId, bool up);

        /// <summary>
        /// Count the testcases.
        /// </summary>
        /// <param name="problemId">The problem ID.</param>
        /// <returns>The count task.</returns>
        Task<int> CountAsync(int problemId);

        /// <summary>
        /// Count and get the total score from the testcases.
        /// </summary>
        /// <param name="problemId">The problem ID.</param>
        /// <returns>The count task.</returns>
        Task<(int, int)> CountAndScoreAsync(int problemId);

        /// <summary>
        /// Get the testcase file.
        /// </summary>
        /// <param name="testcase">The testcase.</param>
        /// <param name="target">The target. (<c>in</c> or <c>out</c>)</param>
        /// <returns>The task for fetching the file, resulting in the <see cref="IBlobInfo"/>.</returns>
        Task<IBlobInfo> GetFileAsync(Testcase testcase, string target);

        /// <summary>
        /// Set the testcase file.
        /// </summary>
        /// <param name="testcase">The testcase.</param>
        /// <param name="target">The target. (<c>in</c> or <c>out</c>)</param>
        /// <param name="source">The source.</param>
        /// <returns>The task for fetching the file, resulting in the <see cref="IBlobInfo"/>.</returns>
        Task<IBlobInfo> SetFileAsync(Testcase testcase, string target, Stream source);
    }
}
