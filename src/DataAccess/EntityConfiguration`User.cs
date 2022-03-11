using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon.Storages
{
    public class PolygonIdentityEntityConfiguration<TUser, TContext> :
        EntityTypeConfigurationSupplier<TContext>,
        IEntityTypeConfiguration<Rejudging>,
        IEntityTypeConfiguration<ProblemAuthor>
        where TUser : class, Microsoft.AspNetCore.Identity.IUser
        where TContext : DbContext
    {
        public void Configure(EntityTypeBuilder<Rejudging> entity)
        {
            entity.HasOne<TUser>()
                .WithMany()
                .HasForeignKey(e => e.IssuedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<TUser>()
                .WithMany()
                .HasForeignKey(e => e.OperatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public void Configure(EntityTypeBuilder<ProblemAuthor> entity)
        {
            entity.HasOne<TUser>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
