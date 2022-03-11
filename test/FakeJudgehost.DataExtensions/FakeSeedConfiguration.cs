using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon
{
    public class FakeSeedConfiguration<TContext> :
        EntityTypeConfigurationSupplier<TContext>,
        IEntityTypeConfiguration<Executable>,
        IEntityTypeConfiguration<Language>
        where TContext : DbContext
    {
        public void Configure(EntityTypeBuilder<Language> entity)
        {
            var more = new[]
            {
                new Language
                {
                    Id = "fake",
                    AllowJudge = true,
                    AllowSubmit = true,
                    CompileScript = "fake",
                    FileExtension = "fake",
                    TimeFactor = 1,
                    Name = "Fake Language",
                }
            };

            entity.HasData(more);
        }

        public void Configure(EntityTypeBuilder<Executable> entity)
        {
            var count = new byte[10];

            var more = new[]
            {
                new Executable
                {
                    Id = "fake",
                    Description = "compiler for fake",
                    Md5sum = count.ToMD5().ToHexDigest(true),
                    ZipSize = count.Length,
                    ZipFile = count,
                    Type = "compile",
                }
            };

            entity.HasData(more);
        }
    }
}
