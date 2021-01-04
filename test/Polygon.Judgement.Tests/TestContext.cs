using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SatelliteSite.IdentityModule.Entities;

namespace SatelliteSite
{
    public class TestContext : IdentityDbContext<User, Role, int>
    {
        public TestContext(DbContextOptions options)
            : base(options)
        {
        }
    }
}
