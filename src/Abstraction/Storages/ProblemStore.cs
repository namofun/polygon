using Microsoft.Extensions.FileProviders;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    /// <summary>
    /// The store interface for <see cref="Problem"/>.
    /// </summary>
    public interface IProblemStore
    {
        /// <summary>
        /// Create an instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The created entity.</returns>
        Task<Problem> CreateAsync(Problem entity);

        /// <summary>
        /// Commits the changes on the instance of entity.
        /// </summary>
        /// <remarks>
        /// This method is not recommended since this may cause concurrency problems.
        /// Preserved for import providers.
        /// </remarks>
        /// <param name="entity">The entity.</param>
        /// <returns>The update task.</returns>
        Task CommitChangesAsync(Problem entity);

        /// <summary>
        /// Update the instance of entity
        /// </summary>
        /// <param name="problem">The problem entity.</param>
        /// <param name="expression">The update expression.</param>
        /// <returns>The update task.</returns>
        Task UpdateAsync(Problem problem, Expression<Func<Problem, Problem>> expression);

        /// <summary>
        /// Delete the instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The delete task.</returns>
        Task DeleteAsync(Problem entity);

        /// <summary>
        /// Toggle the allow judge flag for problem.
        /// </summary>
        /// <param name="probid">The problem ID.</param>
        /// <param name="tobe">The toggle result.</param>
        /// <returns>The toggle task.</returns>
        Task ToggleSubmitAsync(int probid, bool tobe);

        /// <summary>
        /// Toggle the allow judge flag for problem.
        /// </summary>
        /// <param name="probid">The problem ID.</param>
        /// <param name="tobe">The toggle result.</param>
        /// <returns>The toggle task.</returns>
        Task ToggleJudgeAsync(int probid, bool tobe);

        /// <summary>
        /// Find the problem via ID.
        /// </summary>
        /// <param name="problemId">The problem ID.</param>
        /// <returns>The task for problem.</returns>
        Task<Problem?> FindAsync(int problemId);

        /// <summary>
        /// Find the problem via ID.
        /// </summary>
        /// <param name="problemId">The problem ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>The task for problem.</returns>
        Task<(Problem?, AuthorLevel?)> FindAsync(int problemId, int userId);

        /// <summary>
        /// Check the problem permission via ID.
        /// </summary>
        /// <param name="problemId">The problem ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>The task for getting the author level.</returns>
        Task<AuthorLevel?> CheckPermissionAsync(int problemId, int userId);

        /// <summary>
        /// Check the problems.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The task for checking.</returns>
        Task<IEnumerable<(int, string)>> ListPermissionAsync(int userId);

        /// <summary>
        /// List available problems.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="perCount">The count per page.</param>
        /// <param name="ascending">Whether to pagination as ascending.</param>
        /// <param name="uid">The user id.</param>
        /// <param name="leastLevel">The least problem level to show.</param>
        /// <returns>The task for paginated list of problems.</returns>
        Task<IPagedList<Problem>> ListAsync(int page, int perCount, bool ascending = true, int? uid = null, AuthorLevel? leastLevel = null);

        /// <summary>
        /// List problem names.
        /// </summary>
        /// <param name="condition">The conditions.</param>
        /// <returns>The task for fetching names.</returns>
        Task<Dictionary<int, string>> ListNameAsync(Expression<Func<Submission, bool>> condition);

        /// <summary>
        /// List problem names.
        /// </summary>
        /// <param name="condition">The conditions.</param>
        /// <returns>The task for fetching names.</returns>
        Task<Dictionary<int, string>> ListNameAsync(Expression<Func<Problem, bool>> condition);

        /// <summary>
        /// Authorize the problem with such user ID.
        /// </summary>
        /// <param name="problemId">The problem to share.</param>
        /// <param name="userId">The user to authorize.</param>
        /// <param name="level">The newest level to be.</param>
        /// <returns>The task for authorizing.</returns>
        Task AuthorizeAsync(int problemId, int userId, AuthorLevel? level);

        /// <summary>
        /// List permitted users.
        /// </summary>
        /// <param name="probid">The problem ID.</param>
        /// <returns>The list of permitted users.</returns>
        Task<IEnumerable<(int UserId, string UserName, AuthorLevel Level)>> ListPermittedUserAsync(int probid);

        /// <summary>
        /// Write file to problem repository.
        /// </summary>
        /// <param name="problem">The problem.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="content">The content.</param>
        /// <returns>The task for storing files, resulting in <see cref="IBlobInfo"/>.</returns>
        Task<IBlobInfo> WriteFileAsync(Problem problem, string fileName, string content);

        /// <summary>
        /// Get the problem file.
        /// </summary>
        /// <param name="problemId">The problem ID.</param>
        /// <param name="fileName">The file name.</param>
        /// <returns>The task for fetching the file, resulting in the <see cref="IBlobInfo"/>.</returns>
        Task<IBlobInfo> GetFileAsync(int problemId, string fileName);

        /// <summary>
        /// Rebuild the submission statistics.
        /// </summary>
        /// <returns>The task for rebuilding.</returns>
        Task RebuildStatisticsAsync();

        /// <summary>
        /// Read the compiled statement cache HTML for problem.
        /// </summary>
        /// <param name="problemId">The problem to read.</param>
        /// <returns>A <see cref="Task"/> for reading the statement.</returns>
        Task<string?> ReadCompiledHtmlAsync(int problemId);
    }
}
