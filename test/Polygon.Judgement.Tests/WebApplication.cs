using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polygon;
using Polygon.FakeJudgehost;
using SatelliteSite.IdentityModule.Entities;
using SatelliteSite.IdentityModule.Services;
using System.Reflection;
using System.Threading.Tasks;

namespace SatelliteSite.Tests
{
    public class WebApplication : SubstrateApplicationBase, Xunit.IAsyncLifetime
    {
        protected override Assembly EntryPointAssembly => typeof(DefaultContext).Assembly;

        protected override IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .MarkDomain<Program>()
                .AddModule<IdentityModule.IdentityModule<User, Role, TestContext>>()
                .AddModule<PolygonModule.PolygonModule<Polygon.TestRole<User, Role, TestContext>>>()
                .AddDatabaseInMemory<TestContext>("0x8c")
                .ConfigureSubstrateDefaults<TestContext>(b =>
                {
                    b.ConfigureServices(services =>
                    {
                        services.AddJudgehost<InternalErrorActivity>("fake-judgehost-0");
                        services.AddJudgehost<FakeJudgeActivity>("fake-judgehost-1");
                        services.AddJudgehost<FakeJudgeActivity>("fake-judgehost-2");

                        services.ConfigureJudgeDaemon(options =>
                        {
                            options.HttpClientFactory = _ => CreateClient();
                        });
                    });
                });

        protected override void PrepareHost(IHost host) =>
            host.EnsureCreated<TestContext>();

        protected override void CleanupHost(IHost host) =>
            host.EnsureDeleted<TestContext>();

        public Task InitializeAsync()
        {
            return this.RunScoped(async sp =>
            {
                var um = sp.GetRequiredService<IUserManager>();
                var opt = sp.GetRequiredService<IOptions<DaemonOptions>>();
                var newUser = um.CreateEmpty(opt.Value.UserName);
                newUser.Email = "test@test.com";
                await um.CreateAsync(newUser, opt.Value.Password);
                await um.AddToRoleAsync(newUser, "Judgehost");
            });
        }

        public Task DisposeAsync()
        {
            Dispose();
            return Task.CompletedTask;
        }
    }
}
