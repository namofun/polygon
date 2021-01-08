using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polygon.Entities;
using Polygon.Storages;
using System;
using System.Linq;

namespace Polygon
{
    public class FakeSeedConfiguration<TContext> :
        EntityTypeConfigurationSupplier<TContext>,
        IEntityTypeConfiguration<Executable>,
        IEntityTypeConfiguration<Language>
        where TContext : DbContext
    {
        public void Configure(EntityTypeBuilder<Language> entity)
        {
            var more = SeedConfiguration<TContext>
                .GetSeedLanguages()
                .Where(e => e.Id == "cpp")
                .ToList();

            more.Add(new Language
            {
                Id = "fake",
                AllowJudge = true,
                AllowSubmit = true,
                CompileScript = "fake",
                FileExtension = "fake",
                TimeFactor = 1,
                Name = "Fake Language",
            });

            SeedConfiguration<TContext>.ClearUp(more, "Id", e => e.Id, entity.Metadata);
            entity.HasData(more);
        }

        public void Configure(EntityTypeBuilder<Executable> entity)
        {
            var count = new byte[10];

            var more = SeedConfiguration<TContext>
                .GetSeedExecutables()
                .Where(e => e.Type != "compile" || e.Id == "cpp")
                .ToList();

            more.Add(new Executable
            {
                Id = "fake",
                Description = "compiler for fake",
                Md5sum = count.ToMD5().ToHexDigest(true),
                ZipSize = count.Length,
                ZipFile = count,
                Type = "compile",
            });

            SeedConfiguration<TContext>.ClearUp(more, "Id", e => e.Id, entity.Metadata);
            entity.HasData(more);
        }
    }
}
