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
    }
}
