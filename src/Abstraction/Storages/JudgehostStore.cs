using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    /// <summary>
    /// The store interface for <see cref="Judgehost"/>.
    /// </summary>
    public interface IJudgehostStore
    {
        /// <summary>
        /// Create an instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The created entity.</returns>
        Task<Judgehost> CreateAsync(Judgehost entity);

        /// <summary>
        /// Update the instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The update task.</returns>
        Task UpdateAsync(Judgehost entity);

        /// <summary>
        /// Update the instance of entity.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <param name="expression">The update expression.</param>
        /// <returns>The update task.</returns>
        Task UpdateAsync(string id, Expression<Func<Judgehost, Judgehost>> expression);

        /// <summary>
        /// Toggle the activity of judgehost.
        /// </summary>
        /// <param name="hostname">The host name.</param>
        /// <param name="active">The active result.</param>
        /// <returns>The toggle task.</returns>
        Task<int> ToggleAsync(string hostname, bool active);

        /// <summary>
        /// List judgehosts.
        /// </summary>
        /// <returns>The list task.</returns>
        Task<List<Judgehost>> ListAsync();

        /// <summary>
        /// Find the entity.
        /// </summary>
        /// <param name="name">The entity id.</param>
        /// <returns>The find task.</returns>
        Task<Judgehost> FindAsync(string name);

        /// <summary>
        /// Notify the judgehost polled now.
        /// </summary>
        /// <param name="host">The judgehost.</param>
        /// <returns>The task for notifying.</returns>
        Task NotifyPollAsync(Judgehost host);

        /// <summary>
        /// Count the judgehosts in failure.
        /// </summary>
        /// <returns>The task for count.</returns>
        Task<int> CountFailureAsync();

        /// <summary>
        /// Fetch the judgings from hosts.
        /// </summary>
        /// <param name="hostname">The host name.</param>
        /// <param name="page">The page.</param>
        /// <param name="count">The count per page.</param>
        /// <returns>The task for paginated list of judgings.</returns>
        Task<IPagedList<Judging>> FetchJudgingsAsync(string hostname, int page = 1, int count = 50);
    }
}
