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

        public DbSet<Executable> Executables => Set<Executable>();

        public DbSet<InternalError> InternalErrors => Set<InternalError>();

        public DbSet<Judgehost> Judgehosts => Set<Judgehost>();

        public DbSet<Judging> Judgings => Set<Judging>();

        public DbSet<JudgingRun> JudgingRuns => Set<JudgingRun>();

        public DbSet<Language> Languages => Set<Language>();

        public DbSet<Problem> Problems => Set<Problem>();

        public DbSet<Rejudging> Rejudgings => Set<Rejudging>();

        public DbSet<Submission> Submissions => Set<Submission>();

        public DbSet<SubmissionStatistics> SubmissionStatistics => Set<SubmissionStatistics>();

        public DbSet<Testcase> Testcases => Set<Testcase>();

        public DbSet<ProblemAuthor> ProblemAuthors => Set<ProblemAuthor>();
    }
}
