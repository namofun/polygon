using Microsoft.Extensions.FileProviders;

namespace Polygon.Storages
{
    public interface IJudgingFileProvider : IMutableFileProvider
    {
    }

    public interface IProblemFileProvider : IMutableFileProvider
    {
    }

    public class PhysicalPolygonFileProvider : PhysicalMutableFileProvider, IJudgingFileProvider, IProblemFileProvider
    {
        public PhysicalPolygonFileProvider(string path) : base(path)
        {
        }
    }
}
