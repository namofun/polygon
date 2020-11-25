using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polygon.Entities;

namespace Polygon.Storages
{
    /// <summary>
    /// Entity configuration for polygon backend.
    /// </summary>
    /// <typeparam name="TUser">The user type.</typeparam>
    /// <typeparam name="TRole">The role type.</typeparam>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <remarks>Should make entity inclusion on Contest to Rejudging.</remarks>
    public class PolygonEntityConfiguration<TUser, TRole, TContext> :
        EntityTypeConfigurationSupplier<TContext>,
        IEntityTypeConfiguration<Executable>,
        IEntityTypeConfiguration<InternalError>,
        IEntityTypeConfiguration<Judgehost>,
        IEntityTypeConfiguration<Judging>,
        IEntityTypeConfiguration<JudgingRun>,
        IEntityTypeConfiguration<Language>,
        IEntityTypeConfiguration<Problem>,
        IEntityTypeConfiguration<Rejudging>,
        IEntityTypeConfiguration<Submission>,
        IEntityTypeConfiguration<SubmissionStatistics>,
        IEntityTypeConfiguration<Testcase>
        where TContext : DbContext, ISolutionAuthorQueryable
        where TUser : SatelliteSite.IdentityModule.Entities.User
        where TRole : SatelliteSite.IdentityModule.Entities.Role, IRoleWithProblem
    {
        public void Configure(EntityTypeBuilder<Executable> entity)
        {
            entity.ToTable("PolygonExecutables");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("ExecId");

            entity.Property(e => e.Id)
                .IsRequired()
                .HasMaxLength(64)
                .IsUnicode(false);

            entity.Property(e => e.Md5sum)
                .IsRequired()
                .HasMaxLength(32)
                .IsUnicode(false);

            entity.Property(e => e.ZipFile)
                .IsRequired()
                .HasMaxLength(1 << 20);
        }

        public void Configure(EntityTypeBuilder<InternalError> entity)
        {
            entity.ToTable("PolygonErrors");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("ErrorId");

            entity.HasIndex(e => e.Status);

            entity.Property(e => e.Description)
                .IsRequired();

            entity.Property(e => e.Disabled)
                .IsRequired();

            entity.Property(e => e.JudgehostLog)
                .IsRequired();
        }

        public void Configure(EntityTypeBuilder<Judgehost> entity)
        {
            entity.ToTable("PolygonJudgehosts");

            entity.HasKey(e => e.ServerName);

            entity.Property(e => e.ServerName)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(64);
        }

        public void Configure(EntityTypeBuilder<Judging> entity)
        {
            entity.ToTable("PolygonJudgings");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("JudgingId");

            entity.HasOne<Submission>(e => e.s)
                .WithMany(e => e.Judgings)
                .HasForeignKey(e => e.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Judgehost>()
                .WithMany()
                .HasForeignKey(e => e.Server)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Server)
                .IsUnicode(false)
                .HasMaxLength(64);

            entity.HasIndex(e => e.Status);

            entity.Property(e => e.CompileError)
                .IsUnicode(false)
                .HasMaxLength(131072);

            entity.HasOne<Rejudging>()
                .WithMany()
                .HasForeignKey(e => e.RejudgingId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne<Judging>()
                .WithMany()
                .HasForeignKey(e => e.PreviousJudgingId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public void Configure(EntityTypeBuilder<JudgingRun> entity)
        {
            entity.ToTable("PolygonJudgingRuns");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("RunId");

            entity.HasOne<Judging>(e => e.j)
                .WithMany(e => e.Details)
                .HasForeignKey(e => e.JudgingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Testcase>()
                .WithMany()
                .HasForeignKey(e => e.TestcaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.MetaData)
                .IsUnicode(false)
                .HasMaxLength(131072);

            entity.Property(e => e.OutputSystem)
                .IsUnicode(false)
                .HasMaxLength(131072);

            entity.Property(e => e.OutputDiff)
                .IsUnicode(false)
                .HasMaxLength(131072);
        }

        public void Configure(EntityTypeBuilder<Language> entity)
        {
            entity.ToTable("PolygonLanguages");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(16)
                .HasColumnName("LangId");

            entity.Property(e => e.Name)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(32);

            entity.Property(e => e.FileExtension)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(32);

            entity.Property(e => e.CompileScript)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(64);

            entity.HasOne<Executable>()
                .WithMany()
                .HasForeignKey(e => e.CompileScript)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public void Configure(EntityTypeBuilder<Problem> entity)
        {
            entity.ToTable("PolygonProblems");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("ProblemId");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(e => e.Source)
                .HasMaxLength(256);

            entity.Property(e => e.TagName)
                .HasMaxLength(256);

            entity.Property(e => e.RunScript)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(64);

            entity.HasOne<Executable>()
                .WithMany()
                .HasForeignKey(e => e.RunScript)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.CompareScript)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(64);

            entity.HasOne<Executable>()
                .WithMany()
                .HasForeignKey(e => e.CompareScript)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.ComapreArguments)
                .IsUnicode(false)
                .HasMaxLength(128);
        }

        public void Configure(EntityTypeBuilder<Rejudging> entity)
        {
            entity.ToTable("PolygonRejudgings");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("RejudgingId");

            entity.HasIndex(e => e.ContestId);

            // entity.HasOne<Contest>()
            //     .WithMany()
            //     .HasForeignKey(e => e.ContestId)
            //     .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Reason)
                .IsRequired();

            entity.HasOne<TUser>()
                .WithMany()
                .HasForeignKey(e => e.IssuedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<TUser>()
                .WithMany()
                .HasForeignKey(e => e.OperatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(e => e.Issuer);
            entity.Ignore(e => e.Operator);
            entity.Ignore(e => e.Ready);
        }

        public void Configure(EntityTypeBuilder<Submission> entity)
        {
            entity.ToTable("PolygonSubmissions");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("SubmissionId");

            entity.HasIndex(e => e.ContestId);
            entity.HasIndex(e => new { e.ContestId, e.TeamId });
            entity.HasIndex(e => e.Language);
            entity.HasIndex(e => e.ProblemId);
            entity.HasIndex(e => e.RejudgingId);

            entity.HasOne<Problem>(e => e.p)
                .WithMany()
                .HasForeignKey(e => e.ProblemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.SourceCode)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(262144);

            entity.HasOne<Language>(e => e.l)
                .WithMany()
                .HasForeignKey(e => e.Language)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Language)
                .IsRequired()
                .HasMaxLength(16)
                .IsUnicode(false);

            entity.Property(e => e.Ip)
                .IsRequired()
                .HasMaxLength(128)
                .IsUnicode(false);

            entity.HasOne<Rejudging>()
                .WithMany()
                .HasForeignKey(e => e.RejudgingId)
                .OnDelete(DeleteBehavior.SetNull);
        }

        public void Configure(EntityTypeBuilder<SubmissionStatistics> entity)
        {
            entity.ToTable("PolygonStatistics");

            entity.HasKey(e => new { e.ContestId, e.TeamId, e.ProblemId });

            entity.HasIndex(e => e.ProblemId);
            entity.HasIndex(e => e.ContestId);
            entity.HasIndex(e => new { e.ContestId, e.TeamId });
            entity.HasIndex(e => new { e.ContestId, e.ProblemId });

            entity.HasOne<Problem>()
                .WithMany()
                .HasForeignKey(e => e.ProblemId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public void Configure(EntityTypeBuilder<Testcase> entity)
        {
            entity.ToTable("PolygonTestcases");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("TestcaseId");

            entity.HasIndex(e => e.ProblemId);

            entity.HasIndex(e => new { e.ProblemId, e.Rank })
                .IsUnique();

            entity.HasOne<Problem>()
                .WithMany()
                .HasForeignKey(e => e.ProblemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Md5sumInput)
                .HasMaxLength(32)
                .IsRequired()
                .IsUnicode(false);

            entity.Property(e => e.Md5sumOutput)
                .HasMaxLength(32)
                .IsRequired()
                .IsUnicode(false);

            entity.Property(e => e.Description)
                .HasMaxLength(1 << 9)
                .IsRequired();
        }
    }
}
