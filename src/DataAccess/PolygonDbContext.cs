using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    /// <summary>
    /// The marker interface to add polygon <see cref="DbSet{TEntity}"/>s.
    /// </summary>
    public interface IPolygonDbContext
    {
        /// <summary>
        /// Gets the database set for <see cref="Executable"/>.
        /// </summary>
        DbSet<Executable> Executables { get; }

        /// <summary>
        /// Gets the database set for <see cref="InternalError"/>.
        /// </summary>
        DbSet<InternalError> InternalErrors { get; }

        /// <summary>
        /// Gets the database set for <see cref="Judgehost"/>.
        /// </summary>
        DbSet<Judgehost> Judgehosts { get; }

        /// <summary>
        /// Gets the database set for <see cref="Judging"/>.
        /// </summary>
        DbSet<Judging> Judgings { get; }

        /// <summary>
        /// Gets the database set for <see cref="JudgingRun"/>.
        /// </summary>
        DbSet<JudgingRun> JudgingRuns { get; }

        /// <summary>
        /// Gets the database set for <see cref="Language"/>.
        /// </summary>
        DbSet<Language> Languages { get; }

        /// <summary>
        /// Gets the database set for <see cref="Problem"/>.
        /// </summary>
        DbSet<Problem> Problems { get; }

        /// <summary>
        /// Gets the database set for <see cref="Rejudging"/>.
        /// </summary>
        DbSet<Rejudging> Rejudgings { get; }

        /// <summary>
        /// Gets the database set for <see cref="Submission"/>.
        /// </summary>
        DbSet<Submission> Submissions { get; }

        /// <summary>
        /// Gets the database set for <see cref="Polygon.Entities.SubmissionStatistics"/>.
        /// </summary>
        DbSet<SubmissionStatistics> SubmissionStatistics { get; }

        /// <summary>
        /// Gets the database set for <see cref="Testcase"/>.
        /// </summary>
        DbSet<Testcase> Testcases { get; }

        /// <summary>
        /// Gets the database set for <see cref="ProblemAuthor"/>.
        /// </summary>
        DbSet<ProblemAuthor> ProblemAuthors { get; }

        /// <summary>
        /// <para>
        /// Saves all changes made in this context to the database.
        /// </para>
        /// <para>
        /// Multiple active operations on the same context instance are not supported. Use
        /// 'await' to ensure that any asynchronous operations have completed before calling
        /// another method on this context.
        /// </para>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        /// <exception cref="DbUpdateException">An error is encountered while saving to the database.</exception>
        /// <exception cref="DbUpdateConcurrencyException">A concurrency violation is encountered while saving to the database. A concurrency violation occurs when an unexpected number of rows are affected during save. This is usually because the data in the database has been modified since it was loaded into memory.</exception>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
