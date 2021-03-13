using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polygon.Entities;

namespace Polygon.Storages
{
    public class SeedConfiguration<TContext> :
        EntityTypeConfigurationSupplier<TContext>,
        IEntityTypeConfiguration<Executable>,
        IEntityTypeConfiguration<Language>
        where TContext : DbContext
    {
        public void Configure(EntityTypeBuilder<Language> entity)
        {
            entity.HasData(SeedResource.GetSeedLanguages());
        }

        public void Configure(EntityTypeBuilder<Executable> entity)
        {
            entity.HasData(SeedResource.GetSeedExecutables());
        }
    }
}
