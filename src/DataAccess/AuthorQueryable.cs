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
    public interface ISolutionAuthorQueryable
    {
        /// <summary>
        /// Create an <see cref="IQueryable{SolutionAuthor}"/> to query solution authors.
        /// </summary>
        /// <param name="predicate">The submission filter.</param>
        /// <returns>The solution author models.</returns>
        IQueryable<SolutionAuthor> Author(Expression<Func<Submission, bool>> predicate);
    }
}
