using Microsoft.EntityFrameworkCore;

namespace SatelliteSite
{
    public class TestContext : DefaultContext
    {
        public TestContext(DbContextOptions options)
            : base(options)
        {
        }
    }
}
