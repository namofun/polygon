using Microsoft.EntityFrameworkCore;
using Polygon.Entities;

namespace Polygon.Storages
{
    /// <summary>
    /// The marker interface to add polygon <see cref="DbSet{TEntity}"/>s.
    /// </summary>
    public interface IPolygonDbContext
    {
        DbSet<Executable> Executables { get; set; }

        DbSet<InternalError> InternalErrors { get; set; }

        DbSet<Judgehost> Judgehosts { get; set; }

        DbSet<Judging> Judgings { get; set; }

        DbSet<JudgingRun> JudgingRuns { get; set; }

        DbSet<Language> Languages { get; set; }

        DbSet<Problem> Problems { get; set; }

        DbSet<Rejudging> Rejudgings { get; set; }

        DbSet<Submission> Submissions { get; set; }

        DbSet<SubmissionStatistics> SubmissionStatistics { get; set; }

        DbSet<Testcase> Testcases { get; set; }

        DbSet<Testcase> ProblemAuthors { get; set; }
    }
}
