using Xylab.Polygon.Entities;
using System.Linq;

namespace Xylab.Polygon.Storages
{
    /// <summary>
    /// The marker interface to gets the <see cref="IQueryable{T}"/>s.
    /// </summary>
    public interface IPolygonQueryableStore
    {
        /// <summary>
        /// Gets the queryable for <see cref="Executable"/>.
        /// </summary>
        IQueryable<Executable> Executables { get; }

        /// <summary>
        /// Gets the queryable for <see cref="InternalError"/>.
        /// </summary>
        IQueryable<InternalError> InternalErrors { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Judgehost"/>.
        /// </summary>
        IQueryable<Judgehost> Judgehosts { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Judging"/>.
        /// </summary>
        IQueryable<Judging> Judgings { get; }

        /// <summary>
        /// Gets the queryable for <see cref="JudgingRun"/>.
        /// </summary>
        IQueryable<JudgingRun> JudgingRuns { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Language"/>.
        /// </summary>
        IQueryable<Language> Languages { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Problem"/>.
        /// </summary>
        IQueryable<Problem> Problems { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Rejudging"/>.
        /// </summary>
        IQueryable<Rejudging> Rejudgings { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Submission"/>.
        /// </summary>
        IQueryable<Submission> Submissions { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Polygon.Entities.SubmissionStatistics"/>.
        /// </summary>
        IQueryable<SubmissionStatistics> SubmissionStatistics { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Testcase"/>.
        /// </summary>
        IQueryable<Testcase> Testcases { get; }

        /// <summary>
        /// Gets the queryable for <see cref="ProblemAuthor"/>.
        /// </summary>
        IQueryable<ProblemAuthor> ProblemAuthors { get; }
    }
}
