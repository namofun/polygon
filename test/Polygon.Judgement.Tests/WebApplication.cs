using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Polygon;
using Polygon.FakeJudgehost;
using SatelliteSite.IdentityModule.Entities;
using System.Reflection;

namespace SatelliteSite.Tests
{
    public class WebApplication : SubstrateApplicationBase
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
                        services.AddFakeJudgehostAccount();

                        services.ConfigureJudgeDaemon(options =>
                        {
                            options.HttpClientFactory = _ =>
                            {
                                var client = CreateClient();
                                client.BaseAddress = new System.Uri("http://localhost/api/");
                                return client;
                            };
                        });
                    });
                });

        protected override void PrepareHost(IHost host) =>
            host.EnsureCreated<TestContext>();

        protected override void CleanupHost(IHost host) =>
            host.EnsureDeleted<TestContext>();
    }
}
