using Microsoft.Extensions.DependencyInjection;
using Polygon;
using Polygon.Storages;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace SatelliteSite.Tests
{
    public class IntegratedServerTests : IClassFixture<WebApplication>
    {
        private readonly WebApplication _factory;

        public IntegratedServerTests(WebApplication factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Create()
        {
            var client = _factory.CreateClient();

            using (var root = await client.GetAsync("/"))
            {
                Assert.Equal(HttpStatusCode.NotFound, root.StatusCode);
            }
        }

        [Fact]
        public async Task InternalError()
        {
            var judgehost = _factory.Services.GetJudgehost("fake-judgehost-0");

            await _factory.RunScoped(async sp =>
            {
                var list = await sp.GetRequiredService<IJudgehostStore>().ListAsync();
                Assert.Empty(list);

                var iec = await sp.GetRequiredService<IInternalErrorStore>().CountOpenAsync();
                Assert.Equal(0, iec);
            });

            await judgehost.ManualStartAsync(default);

            await ((Polygon.FakeJudgehost.InternalErrorActivity)judgehost.Strategy).Semaphore.WaitAsync();

            await _factory.RunScoped(async sp =>
            {
                var list = await sp.GetRequiredService<IJudgehostStore>().ListAsync();
                var item = Assert.Single(list);
                Assert.Equal(judgehost.HostName, item.ServerName);
                Assert.False(item.Active);

                var iec = await sp.GetRequiredService<IInternalErrorStore>().CountOpenAsync();
                Assert.Equal(1, iec);
            });
        }
    }
}
