using Markdig;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polygon.FakeJudgehost;
using Polygon.Storages;
using SatelliteSite.IdentityModule.Entities;
using System.IO;

namespace SatelliteSite
{
    public class Program
    {
        public static IHost Current { get; private set; }

        public static void Main(string[] args)
        {
            Current = CreateHostBuilder(args).Build();
            Current.AutoMigrate<DefaultContext>();
            Current.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .MarkDomain<Program>()
                .AddModule<IdentityModule.IdentityModule<User, Role, DefaultContext>>()
                .AddModule<PolygonModule.PolygonModule<Polygon.DefaultRole<DefaultContext, QueryCache<DefaultContext>>>>()
                .AddModule<TelemetryModule.TelemetryModule>()
                .AddModule<HostModule>()
                .AddDatabase<DefaultContext>((c, b) => b.UseSqlServer(c.GetConnectionString("UserDbConnection"), b => b.UseBulk()))
                .ConfigureSubstrateDefaults<DefaultContext>(builder =>
                {
                    builder.ConfigureServices((context, services) =>
                    {
                        services.AddMarkdown();
                        services.AddDbModelSupplier<DefaultContext, PolygonIdentityEntityConfiguration<User, DefaultContext>>();
                        //services.AddDbModelSupplier<DefaultContext, SeedConfiguration<DefaultContext>>();

                        services.ConfigurePolygonStorage(options =>
                        {
                            options.JudgingDirectory = Path.Combine(context.HostingEnvironment.ContentRootPath, "Runs");
                            options.ProblemDirectory = Path.Combine(context.HostingEnvironment.ContentRootPath, "Problems");
                        });

                        /*
                        services.AddFakeJudgehost()
                            .AddJudgehost<FakeJudgeActivity>("judgehost-0")
                            .AddHttpClientFactory(sp =>
                            {
                                return new System.Net.Http.HttpClient()
                                {
                                    BaseAddress = new System.Uri("https://localhost:41359/api/")
                                };
                            });
                        */
                    });
                });
    }
}
