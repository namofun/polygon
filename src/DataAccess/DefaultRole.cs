using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Storages;
using Polygon.Storages.Handlers;
using SatelliteSite.IdentityModule.Entities;
using System.IO;

namespace Polygon
{
    public class DefaultRole<TUser, TRole, TContext> : IServiceRole
        where TUser : User, new()
        where TRole : Role, new()
        where TContext : DbContext, IPolygonQueryable
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
            services.AddDbModelSupplier<TContext, PolygonEntityConfiguration<TUser, TContext>>();
            services.AddPolygonStorage<PolygonFacade<TUser, TContext>>();
            services.AddSingleton<QueryCache<TContext>>();

            services.AddMediatRAssembly(typeof(Auditlogging).Assembly);

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
