using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polygon.Storages;
using Polygon.Storages.Handlers;
using System.IO;

namespace Polygon
{
    public class DefaultRole<TContext, TQueryCache> : IServiceRole
        where TContext : DbContext, IPolygonDbContext
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
            services.AddMediatRAssembly(typeof(Auditlogging).Assembly);

            services.AddOptions<PolygonPhysicalOptions>()
                .PostConfigure(options =>
                {
                    if (options.JudgingFileProvider == null)
                    {
                        EnsureDirectoryExists(options.JudgingDirectory);
                    }

                    if (options.ProblemFileProvider == null)
                    {
                        EnsureDirectoryExists(options.ProblemDirectory);
                    }
                });

            services.AddSingleton<IJudgingFileProvider>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<PolygonPhysicalOptions>>();
                return options.Value.JudgingFileProvider ?? new PhysicalPolygonFileProvider(options.Value.JudgingDirectory);
            });

            services.AddSingleton<IProblemFileProvider>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<PolygonPhysicalOptions>>();
                return options.Value.ProblemFileProvider ?? new PhysicalPolygonFileProvider(options.Value.ProblemDirectory);
            });
        }
    }
}
