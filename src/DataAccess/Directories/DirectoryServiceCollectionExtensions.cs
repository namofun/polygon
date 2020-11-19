using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Polygon.Storages
{
    public static class DirectoryServiceCollectionExtensions
    {
        public static OptionsBuilder<PolygonStorageOption> AddPolygonFileDirectory(this IServiceCollection services)
        {
            services.AddSingleton<IJudgingFileProvider, ByOptionJudgingFileProvider>();
            services.AddSingleton<IProblemFileProvider, ByOptionProblemFileProvider>();
            return services.AddOptions<PolygonStorageOption>();
        }
    }
}
