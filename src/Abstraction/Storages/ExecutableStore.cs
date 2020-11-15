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
    /// The store interface for <see cref="Executable"/>.
    /// </summary>
    public interface IExecutableStore
    {
        /// <summary>
        /// Create an instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The created entity.</returns>
        Task<Executable> CreateAsync(Executable entity);

        /// <summary>
        /// Update the instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The update task.</returns>
        Task UpdateAsync(Executable entity);

        /// <summary>
        /// Update the instance of entity.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <param name="expression">The update expression.</param>
        /// <returns>The update task.</returns>
        Task UpdateAsync(string id, Expression<Func<Executable, Executable>> expression);

        /// <summary>
        /// Delete the instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The delete task.</returns>
        Task DeleteAsync(Executable entity);

        /// <summary>
        /// Find the entity.
        /// </summary>
        /// <param name="execid">The entity id.</param>
        /// <returns>The find task.</returns>
        Task<Executable> FindAsync(string execid);

        /// <summary>
        /// List the executable by types.
        /// </summary>
        /// <param name="type">The executable type.</param>
        /// <returns>The executable list.</returns>
        Task<List<Executable>> ListAsync(string? type = null);

        /// <summary>
        /// Find MD5 for each target executable.
        /// </summary>
        /// <param name="targets">The executable IDs.</param>
        /// <returns>The md5 dictionary.</returns>
        Task<Dictionary<string, string>> ListMd5Async(params string[] targets);

        /// <summary>
        /// List the usage for executables.
        /// </summary>
        /// <param name="id">The executable ID.</param>
        /// <returns>The lookup for usages.</returns>
        Task<ILookup<string, string>> ListUsageAsync(string id);

        /// <summary>
        /// Fetch the executable content.
        /// </summary>
        /// <param name="executable">The executable entity.</param>
        /// <returns>The content files.</returns>
        Task<IReadOnlyList<ExecutableContent>> FetchContentAsync(Executable executable);
    }
}
