using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Models;

namespace Xylab.Polygon.Storages
{
    /// <summary>
    /// The store interface for <see cref="Rejudging"/>.
    /// </summary>
    public interface IRejudgingStore
    {
        /// <summary>
        /// Create an instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The created entity.</returns>
        Task<Rejudging> CreateAsync(Rejudging entity);

        /// <summary>
        /// Delete the instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The delete task.</returns>
        Task DeleteAsync(Rejudging entity);

        /// <summary>
        /// Find the rejudging for certain contest.
        /// </summary>
        /// <param name="contestId">The contest ID.</param>
        /// <param name="rejudgingId">The rejudging ID.</param>
        /// <returns>The task for fetching rejudging entity.</returns>
        Task<Rejudging?> FindAsync(int contestId, int rejudgingId);

        /// <summary>
        /// List the rejudging for certain contest.
        /// </summary>
        /// <param name="contestId">The contest ID.</param>
        /// <param name="includeStat">Whether to include statistics about undone rejudging progress.</param>
        /// <returns>The task for fetching rejudging entities.</returns>
        Task<List<Rejudging>> ListAsync(int contestId, bool includeStat = true);

        /// <summary>
        /// View the rejudging difference.
        /// </summary>
        /// <param name="rejudge">The rejudging entity.</param>
        /// <param name="filter">The fetching condition.</param>
        /// <returns>The task for fetching difference.</returns>
        Task<IEnumerable<RejudgingDifference>> ViewAsync(Rejudging rejudge, Expression<Func<Judging, Judging, Submission, bool>>? filter = null);

        /// <summary>
        /// Rejudging several submissions with existing rejudging entity.
        /// </summary>
        /// <param name="predicate">The submissions to rejudge.</param>
        /// <param name="rejudging">The rejudging entity.</param>
        /// <param name="fullTest">Whether to take a full test.</param>
        /// <param name="immediateApply">Whether to apply the rejudging immediately.</param>
        /// <param name="stageAsRunning">Whether to stage the rejudging as running state.</param>
        /// <returns>The task for batch rejudge submissions, returning the count of submissions being rejudged.</returns>
        Task<int> BatchRejudgeAsync(
            Expression<Func<Submission, Judging, bool>> predicate,
            Rejudging rejudging,
            bool fullTest = false,
            bool immediateApply = false,
            bool stageAsRunning = false);

        /// <summary>
        /// Count the undone rejudgings for certain contest.
        /// </summary>
        /// <param name="contestId">The contest ID.</param>
        /// <returns>The task for counting.</returns>
        Task<int> CountUndoneAsync(int contestId);

        /// <summary>
        /// Cancel the rejudging.
        /// </summary>
        /// <param name="rejudge">The rejudging entity.</param>
        /// <param name="uid">The operator user ID.</param>
        /// <returns>The task for cancelling.</returns>
        Task CancelAsync(Rejudging rejudge, int uid);

        /// <summary>
        /// Apply the rejudging.
        /// </summary>
        /// <param name="rejudge">The rejudging entity.</param>
        /// <param name="uid">The operator user ID.</param>
        /// <returns>The task for applying.</returns>
        Task ApplyAsync(Rejudging rejudge, int uid);
    }
}
