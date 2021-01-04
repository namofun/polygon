using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Storages;
using Polygon.Storages.Handlers;
using System.IO;

namespace Polygon
{
    public class DefaultRole<TContext, TQueryCache> : IServiceRole
        where TContext : DbContext
        where TQueryCache : QueryCacheBase<TContext>
    {
        private static void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public void Configure(IServiceCollection services)
        {
            services.AddDbModelSupplier<TContext, PolygonEntityConfiguration<TContext>>();
            services.AddPolygonStorage<PolygonFacade<TContext, TQueryCache>>();
            services.AddSingleton<TQueryCache>();

            MediatR.Registration.ServiceRegistrar.AddMediatRClasses(services, new[] { typeof(Auditlogging).Assembly });

            services.AddOptions<PolygonPhysicalOptions>()
                .PostConfigure(options =>
                {
                    EnsureDirectoryExists(options.JudgingDirectory);
                    EnsureDirectoryExists(options.ProblemDirectory);
                });

            services.AddSingleton<IJudgingFileProvider, ByOptionJudgingFileProvider>();
            services.AddSingleton<IProblemFileProvider, ByOptionProblemFileProvider>();
        }
    }
}
