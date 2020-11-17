using Polygon.Entities;
using Polygon.Models;
using System.Threading.Tasks;

namespace Polygon.Packaging
{
    /// <summary>
    /// The statement provider interface.
    /// </summary>
    public interface IStatementProvider
    {
        /// <summary>
        /// Read the statement for problem.
        /// </summary>
        /// <param name="problem">The problem to read.</param>
        /// <returns>A <see cref="Task"/> for reading the statement.</returns>
        Task<Statement> ReadAsync(Problem problem);
    }
}
