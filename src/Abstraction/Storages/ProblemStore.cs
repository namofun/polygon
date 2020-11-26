﻿using Microsoft.Extensions.FileProviders;
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
        /// Update the instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The update task.</returns>
        Task UpdateAsync(Problem entity);

        /// <summary>
        /// Update the instance of entity
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <param name="expression">The update expression.</param>
        /// <returns>The update task.</returns>
        Task UpdateAsync(int id, Expression<Func<Problem, Problem>> expression);

        /// <summary>
        /// Delete the instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The delete task.</returns>
        Task DeleteAsync(Problem entity);

        /// <summary>
        /// Toggle the allow judge flag for problem.
        /// </summary>
        /// <param name="pid">The problem ID.</param>
        /// <param name="tobe">The toggle result.</param>
        /// <returns>The toggle task.</returns>
        Task ToggleSubmitAsync(int pid, bool tobe);

        /// <summary>
        /// Toggle the allow judge flag for problem.
        /// </summary>
        /// <param name="pid">The problem ID.</param>
        /// <param name="tobe">The toggle result.</param>
        /// <returns>The toggle task.</returns>
        Task ToggleJudgeAsync(int pid, bool tobe);

        /// <summary>
        /// Find the problem via ID.
        /// </summary>
        /// <param name="problemId">The problem ID.</param>
        /// <returns>The task for problem.</returns>
        Task<Problem> FindAsync(int problemId);

        /// <summary>
        /// List available problems.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="perCount">The count per page.</param>
        /// <param name="uid">The user id.</param>
        /// <returns>The task for paginated list of problems.</returns>
        Task<IPagedList<Problem>> ListAsync(int page, int perCount, int? uid = null);

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
        /// List permitted users.
        /// </summary>
        /// <param name="pid">The problem ID.</param>
        /// <returns>The list of permitted users.</returns>
        Task<IEnumerable<(int UserId, string UserName, string NickName)>> ListPermittedUserAsync(int pid);

        /// <summary>
        /// Write file to problem repository.
        /// </summary>
        /// <param name="problem">The problem.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="content">The content.</param>
        /// <returns>The task for storing files, resulting in <see cref="IFileInfo"/>.</returns>
        Task<IFileInfo> WriteFileAsync(Problem problem, string fileName, string content);

        /// <summary>
        /// Write file to problem repository.
        /// </summary>
        /// <param name="problem">The problem.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="content">The content.</param>
        /// <returns>The task for storing files, resulting in <see cref="IFileInfo"/>.</returns>
        Task<IFileInfo> WriteFileAsync(Problem problem, string fileName, byte[] content);

        /// <summary>
        /// Write file to problem repository.
        /// </summary>
        /// <param name="problem">The problem.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="content">The content.</param>
        /// <returns>The task for storing files, resulting in <see cref="IFileInfo"/>.</returns>
        Task<IFileInfo> WriteFileAsync(Problem problem, string fileName, Stream content);

        /// <summary>
        /// Get the problem file.
        /// </summary>
        /// <param name="problemId">The problem ID.</param>
        /// <param name="fileName">The file name.</param>
        /// <returns>The task for fetching the file, resulting in the <see cref="IFileInfo"/>.</returns>
        Task<IFileInfo> GetFileAsync(int problemId, string fileName);

        /// <summary>
        /// Rebuild the submission statistics.
        /// </summary>
        /// <returns>The task for rebuilding.</returns>
        Task RebuildStatisticsAsync();
    }
}
