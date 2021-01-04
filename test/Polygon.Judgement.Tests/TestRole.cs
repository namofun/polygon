using Markdig;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polygon;
using Polygon.Entities;
using Polygon.Storages;
using Polygon.Storages.Handlers;
using SatelliteSite.Tests;

namespace SatelliteSite
{
    public class TestRole : IServiceRole
    {
        private class InMemoryProblemFileProvider : InMemoryMutableFileProvider, IProblemFileProvider { }

        private class InMemoryJudgingFileProvider : InMemoryMutableFileProvider, IJudgingFileProvider { }

        public void Configure(IServiceCollection services)
        {
            services.AddDbModelSupplier<TestContext, PolygonEntityConfiguration<TestContext>>();
            services.AddDbModelSupplier<TestContext, SeedConfiguration<TestContext>>();
            services.AddPolygonStorage<PolygonFacade<TestContext, TestQueryCache>>();
            services.AddSingleton<TestQueryCache>();

            services.AddMarkdown();

            MediatR.Registration.ServiceRegistrar.AddMediatRClasses(services, new[] { typeof(Auditlogging).Assembly });

            services.AddSingleton<IJudgingFileProvider, InMemoryJudgingFileProvider>();
            services.AddSingleton<IProblemFileProvider, InMemoryProblemFileProvider>();
        }
    }
}
