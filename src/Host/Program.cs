using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Polygon.Entities;
using SatelliteSite.IdentityModule.Entities;

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
                .ConfigureSubstrateDefaults<DefaultContext>();
    }
}
