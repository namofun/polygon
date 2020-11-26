using Markdig;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Polygon;
using Polygon.Entities;
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
                .AddModule<IdentityModule.IdentityModule<User, AspNetRole, DefaultContext>>()
                .AddModule<PolygonModule.PolygonModule<User, AspNetRole, DefaultContext>>()
                //.ConfigureServices(services => services.AddDbModelSupplier<DefaultContext, SeedConfiguration<DefaultContext>>())
                .AddDatabaseMssql<DefaultContext>("UserDbConnection")
                .ConfigureSubstrateDefaults<DefaultContext>(builder =>
                {
                    builder.ConfigureServices((context, services) =>
                    {
                        services.AddMarkdown();
                        services.Configure<PolygonOptions>(options =>
                        {
                            options.JudgingDirectory = Path.Combine(context.HostingEnvironment.ContentRootPath, "Runs");
                            options.ProblemDirectory = Path.Combine(context.HostingEnvironment.ContentRootPath, "Problems");
                        });
                    });
                });
    }
}
