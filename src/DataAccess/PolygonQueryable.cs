using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using Polygon.Models;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Polygon.Storages
{
    /// <summary>
    /// Provides an interface to query submission author.
    /// </summary>
    public interface IPolygonQueryable
    {
        /// <summary>
        /// Create an <see cref="IQueryable{SolutionAuthor}"/> to query solution authors.
        /// </summary>
        /// <param name="predicate">The submission filter.</param>
        /// <returns>The solution author models.</returns>
        /// <remarks>
        /// Returning a query like these:
        /// <para>
        /// <c>from s in Submissions.WhereIf(predicate != null, predicate)</c><br />
        /// <c>join u in Users on new { s.ContestId, s.TeamId } equals new { ContestId = 0, TeamId = u.Id }</c><br />
        /// <c>into uu from u in uu.DefaultIfEmpty()</c><br />
        /// <c>join t in Teams on new { s.ContestId, s.Author } equals new { t.ContestId, Author = t.TeamId }</c><br />
        /// <c>into tt from t in tt.DefaultIfEmpty()</c><br />
        /// <c>select new SolutionAuthor(s.Id, s.ContestId, s.TeamId, u.UserName, t.TeamName);</c>
        /// </para>
        /// </remarks>
        IQueryable<SolutionAuthor> Author(Expression<Func<Submission, bool>> predicate);

        /// <summary>
        /// Create an <see cref="IQueryable{JudgehostLoad}"/> to query judgehost loads.
        /// </summary>
        /// <remarks>
        /// Returning a query like these:
        /// <para>
        /// <c>from j in INPUT(DbSet&lt;Judging&gt;)</c><br />
        /// <c>where j.Server != null</c><br />
        /// <c>where j.StopTime > INPUT(DateTimeOffset) || j.StopTime == null</c><br />
        /// <c>let endTime = j.StopTime ?? DateTimeOffset.Now</c><br />
        /// <c>let startTime = j.StartTime ?? INPUT(DateTimeOffset)</c><br />
        /// <c>group (endTime - startTime).TotalSeconds by j.Server into g</c><br />
        /// <c>select new { HostName = g.Key, Load = g.Sum(), Type = INPUT(int) };</c>
        /// </para>
        /// </remarks>
        Expression<Func<DbSet<Judging>, int, DateTimeOffset, IQueryable<JudgehostLoad>>> JudgehostLoadQuery { get; }
    }
}
