using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
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
                .MarkTest(this)
                .AddModule<IdentityModule.IdentityModule<User, Role, TestContext>>()
                .EnableIdentityModuleBasicAuthentication()
                .AddModule<PolygonModule.PolygonModule<TestRole>>()
                .AddDatabase<TestContext>(b => b.UseInMemoryDatabase("0x8c", b => b.UseBulk()))
                .ConfigureSubstrateDefaults<TestContext>(b =>
                {
                    b.ConfigureServices(services =>
                    {
                        services.AddFakeJudgehost()
                            .AddFakeAccount()
                            .AddJudgehost<InternalErrorActivity>("fake-judgehost-0")
                            .AddJudgehost<FakeJudgeActivity>("fake-judgehost-1")
                            .AddJudgehost<FakeJudgeActivity>("fake-judgehost-2")
                            .AddJudgehost<FakeJudgeActivity>("fake-judgehost-3")
                            .AddJudgehost<FakeJudgeActivity>("fake-judgehost-4")
                            .AddFakeSeeds<TestContext>()
                            .AddHttpClientFactory(_ =>
                            {
                                var client = CreateClient();
                                client.BaseAddress = new System.Uri("http://localhost/api/");
                                return client;
                            });
                    });
                });

        protected override void PrepareHost(IHost host) =>
            host.EnsureCreated<TestContext>();

        protected override void CleanupHost(IHost host) =>
            host.EnsureDeleted<TestContext>();
    }
}
