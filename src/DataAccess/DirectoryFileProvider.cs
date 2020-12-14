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

    public class ByOptionJudgingFileProvider : PhysicalMutableFileProvider, IJudgingFileProvider
    {
        public ByOptionJudgingFileProvider(IOptions<PolygonPhysicalOptions> options) : base(options.Value.JudgingDirectory)
        {
        }
    }

    public class ByOptionProblemFileProvider : PhysicalMutableFileProvider, IProblemFileProvider
    {
        public ByOptionProblemFileProvider(IOptions<PolygonPhysicalOptions> options) : base(options.Value.ProblemDirectory)
        {
        }
    }
}
