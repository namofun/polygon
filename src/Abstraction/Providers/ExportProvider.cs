using Polygon.Entities;
using System.IO;
using System.Threading.Tasks;

namespace Polygon.Providers
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
        /// <returns>A <see cref="ValueTask"/> for exporting results.</returns>
        ValueTask<(Stream stream, string mime, string fileName)> ExportAsync(Problem problem);
    }
}
