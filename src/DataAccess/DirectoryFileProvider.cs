using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace Polygon.Storages
{
    public interface IJudgingFileProvider : IBlobProvider
    {
    }

    public interface IProblemFileProvider : IBlobProvider
    {
    }

    public class PhysicalPolygonFileProvider : PhysicalBlobProvider, IJudgingFileProvider, IProblemFileProvider
    {
        public PhysicalPolygonFileProvider(string path) : base(path)
        {
        }
    }
}
