using Polygon.Entities;
using System.Threading.Tasks;

namespace Polygon.Packaging
{
    /// <summary>
    /// The provider interface for exporting problems from system.
    /// </summary>
    public interface IExportProvider
    {
        /// <summary>
        /// Export the problem.
        /// </summary>
        /// <param name="problem">The problem entity.</param>
        /// <returns>A <see cref="Task"/> for exporting results.</returns>
        Task<ExportResult> ExportAsync(Problem problem);
    }
}
