using Markdig;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xylab.Polygon;
using Xylab.Polygon.Storages;
using Xylab.Polygon.Storages.Handlers;
using SatelliteSite.Tests;

namespace SatelliteSite
{
    public class TestRole : IServiceRole
    {
        public void Configure(IServiceCollection services)
        {
            services.AddDbModelSupplier<TestContext, PolygonEntityConfiguration<TestContext>>();
            services.AddDbModelSupplier<TestContext, SeedConfiguration<TestContext>>();
            services.AddPolygonStorage<PolygonFacade<TestContext, TestQueryCache>>();
            services.AddSingleton<TestQueryCache>();

            services.AddMarkdown();

            services.AddMediatRAssembly(typeof(Auditlogging).Assembly);
            services.AddSingleton<IJudgingFileProvider>(new PolygonFileProvider(new InMemoryFileProvider()));
            services.AddSingleton<IProblemFileProvider>(new PolygonFileProvider(new InMemoryFileProvider()));
        }
    }
}
