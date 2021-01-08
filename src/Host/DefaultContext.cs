using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using Polygon.Storages;
using SatelliteSite.IdentityModule.Entities;

namespace SatelliteSite
{
    public class DefaultContext : IdentityDbContext<User, Role, int>, IPolygonDbContext
    {
        public DefaultContext(DbContextOptions options)
            : base(options)
        {
        }

        public virtual DbSet<Executable> Executables { get; set; }

        public virtual DbSet<InternalError> InternalErrors { get; set; }

        public virtual DbSet<Judgehost> Judgehosts { get; set; }

        public virtual DbSet<Judging> Judgings { get; set; }

        public virtual DbSet<JudgingRun> JudgingRuns { get; set; }

        public virtual DbSet<Language> Languages { get; set; }

        public virtual DbSet<Problem> Problems { get; set; }

        public virtual DbSet<Rejudging> Rejudgings { get; set; }

        public virtual DbSet<Submission> Submissions { get; set; }

        public virtual DbSet<SubmissionStatistics> SubmissionStatistics { get; set; }

        public virtual DbSet<Testcase> Testcases { get; set; }

        public virtual DbSet<Testcase> ProblemAuthors { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Rejudging>(entity =>
            {
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.IssuedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.OperatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ProblemAuthor>(entity =>
            {
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
