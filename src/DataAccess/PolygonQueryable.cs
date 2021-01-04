using Polygon.Entities;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    /// <summary>
    /// Provides an interface to query submission author.
    /// </summary>
    public interface IPolygonQueryable
    {
        /// <summary>
        /// Create a task to fetch <see cref="SolutionAuthor"/>s.
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
        Task<List<SolutionAuthor>> FetchAuthorAsync(Expression<Func<Submission, bool>> predicate);

        /// <summary>
        /// Create an expression to calculate the duration seconds between two <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <remarks>
        /// Usually, it can be expressed in several ways.
        /// <list type="bullet">For SqlServer, it may be <c>(start, end) => EF.Functions.DateDiffMillisecond(start, end) / 1000.0</c>.</list>
        /// <list type="bullet">For InMemory, it may be <c>(start, end) => (end - start).TotalSeconds</c>.</list>
        /// </remarks>
        Expression<Func<DateTimeOffset, DateTimeOffset, double>> CalculateDuration { get; }
    }
}
