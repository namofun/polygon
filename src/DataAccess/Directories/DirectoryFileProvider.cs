using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Polygon.Storages
{
    public interface IJudgingFileProvider : IMutableFileProvider
    {
    }

    public interface IProblemFileProvider : IMutableFileProvider
    {
    }

    internal class ByOptionJudgingFileProvider : PhysicalMutableFileProvider, IJudgingFileProvider
    {
        public ByOptionJudgingFileProvider(IOptions<PolygonStorageOption> options) : base(options.Value.JudgingDirectory)
        {
        }
    }

    internal class ByOptionProblemFileProvider : PhysicalMutableFileProvider, IProblemFileProvider
    {
        public ByOptionProblemFileProvider(IOptions<PolygonStorageOption> options) : base(options.Value.ProblemDirectory)
        {
        }
    }
}
