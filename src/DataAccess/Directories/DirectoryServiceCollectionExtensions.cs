using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IO;

namespace Polygon.Storages
{
    public static class DirectoryServiceCollectionExtensions
    {
        private static void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static OptionsBuilder<PolygonStorageOption> AddPolygonFileDirectory(this IServiceCollection services)
        {
            services.AddSingleton<IJudgingFileProvider, ByOptionJudgingFileProvider>();
            services.AddSingleton<IProblemFileProvider, ByOptionProblemFileProvider>();

            return services.AddOptions<PolygonStorageOption>()
                .PostConfigure(options =>
                {
                    EnsureDirectoryExists(options.JudgingDirectory);
                    EnsureDirectoryExists(options.ProblemDirectory);
                });
        }
    }
}
