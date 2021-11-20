using Polygon.Entities;
using Polygon.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    /// <summary>
    /// The store interface for <see cref="InternalError"/>.
    /// </summary>
    public interface IInternalErrorStore
    {
        /// <summary>
        /// Create an instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The created entity.</returns>
        Task<InternalError> CreateAsync(InternalError entity);

        /// <summary>
        /// Find the entity.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <returns>The find task.</returns>
        Task<InternalError?> FindAsync(int id);

        /// <summary>
        /// Mark the internal error as resolved status.
        /// </summary>
        /// <param name="error">The internal error.</param>
        /// <param name="status">The target error status.</param>
        /// <returns>The task for disabled object. If status is <see cref="InternalErrorStatus.Resolved"/>, then return the disabled entity.</returns>
        Task<InternalErrorDisable?> ResolveAsync(InternalError error, InternalErrorStatus status);

        /// <summary>
        /// Count the open internal error alerts.
        /// </summary>
        /// <returns>The task for count.</returns>
        Task<int> CountOpenAsync();

        /// <summary>
        /// List internal errors.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="count">The count per page.</param>
        /// <returns>The task for paginated list of internal errors.</returns>
        Task<IPagedList<InternalError>> ListAsync(int page = 1, int count = 50);
    }
}
