using Markdig;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Storages;
using Polygon.Storages.Handlers;
using SatelliteSite.IdentityModule.Entities;
using SatelliteSite.Tests;

namespace Polygon
{
    public class TestRole<TUser, TRole, TContext> : IServiceRole
        where TUser : User, new()
        where TRole : Role, new()
        where TContext : DbContext, IPolygonQueryable
    {
        private class InMemoryProblemFileProvider : InMemoryMutableFileProvider, IProblemFileProvider { }

        private class InMemoryJudgingFileProvider : InMemoryMutableFileProvider, IJudgingFileProvider { }

        public void Configure(IServiceCollection services)
        {
            services.AddDbModelSupplier<TContext, PolygonEntityConfiguration<TUser, TContext>>();
            services.AddDbModelSupplier<TContext, Entities.SeedConfiguration<TContext>>();
            services.AddDbModelSupplier<TContext, FakeSeedConfiguration<TContext>>();
            services.AddPolygonStorage<PolygonFacade<TUser, TContext>>();
            services.AddSingleton<QueryCache<TContext>>();

            services.AddMarkdown();

            MediatR.Registration.ServiceRegistrar.AddMediatRClasses(services, new[] { typeof(Auditlogging).Assembly });

            services.AddSingleton<IJudgingFileProvider, InMemoryJudgingFileProvider>();
            services.AddSingleton<IProblemFileProvider, InMemoryProblemFileProvider>();
        }
    }
}
