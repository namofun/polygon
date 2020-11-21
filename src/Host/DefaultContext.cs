﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SatelliteSite.IdentityModule.Entities;

namespace SatelliteSite
{
    public class DefaultContext : IdentityDbContext<User, AspNetRole, int>
    {
        public DefaultContext(DbContextOptions options)
            : base(options)
        {
        }
    }
}
