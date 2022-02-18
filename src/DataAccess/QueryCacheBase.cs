using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Entities;
using Polygon.Events;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    /// <summary>
    /// The query cache base for providing external queries.
    /// </summary>
    /// <remarks>
    /// This service should be <see cref="ServiceLifetime.Singleton"/>, and can't rely on any scoped services.
    /// If you want to utilize other scoped services, it is preferred to take action with IHttpContextAccessor.
    /// </remarks>
    public abstract class QueryCacheBase<TContext> : IDbModelSupplier<TContext> where TContext : DbContext, IPolygonDbContext
    {
        private readonly Func<TContext, IAsyncEnumerable<JudgehostLoad>> _judgehostLoad;
        private readonly IAsyncLock _judgementDequeueGuard;

        /// <summary>
        /// Initialize the QueryCacheBase.
        /// </summary>
        /// <param name="durationExpression">
        /// Expression to calculate the duration seconds between two <see cref="DateTimeOffset"/> that can be translated by Entity Framework Core.
        /// <list type="bullet">For SqlServer, it may be <c>(start, end) => EF.Functions.DateDiffMillisecond(start, end) / 1000.0</c>.</list>
        /// <list type="bullet">For InMemory, it may be <c>(start, end) => (end - start).TotalSeconds</c>.</list>
        /// <list type="bullet">For PostgreSQL, it may be <c>(start, end) => PostgresTimeDiff.ExtractEpochFromAge(end, start)</c> with customized DbFunctions.</list>
        /// </param>
        /// <param name="judgementDequeueGuard">
        /// The concurrency guard used when dequeue judgements.
        /// </param>
        public QueryCacheBase(
            Expression<Func<DateTimeOffset, DateTimeOffset, double>> durationExpression,
            IAsyncLock? judgementDequeueGuard = null)
        {
            _judgehostLoad = EF.CompileAsyncQuery(CreateJudgehostLoadQuery(durationExpression));
            _judgementDequeueGuard = judgementDequeueGuard ?? new AsyncLock();
        }

        /// <inheritdoc cref="IDbModelSupplier{TContext}.Configure(ModelBuilder, TContext)" />
        protected virtual void ConfigureModel(ModelBuilder builder, TContext context)
        {
        }

        /// <inheritdoc />
        void IDbModelSupplier<TContext>.Configure(ModelBuilder builder, TContext context)
        {
            ConfigureModel(builder, context);
        }

        /// <summary>
        /// Create a task to fetch <see cref="JudgehostLoad"/>s.
        /// </summary>
        /// <param name="context">The query database context.</param>
        /// <returns>The judgehost load models.</returns>
        public virtual async Task<List<JudgehostLoad>> FetchJudgehostLoadAsync(TContext context)
        {
            var results = new List<JudgehostLoad>();
            await foreach (var item in _judgehostLoad(context))
            {
                results.Add(item);
            }

            return results;
        }

        /// <summary>
        /// Create a task to fetch <see cref="SolutionAuthor"/>s.
        /// </summary>
        /// <param name="context">The query database context.</param>
        /// <param name="predicate">The submission filter.</param>
        /// <returns>The solution author models.</returns>
        /// <remarks>
        /// Returning a query like these:
        /// <para>
        /// <c>from s in Submissions.WhereIf(predicate != null, predicate)</c><br />
        /// <c>join u in Users on new { s.ContestId, s.TeamId } equals new { ContestId = 0, TeamId = u.Id }</c><br />
        /// <c>into uu from u in uu.DefaultIfEmpty()</c><br />
        /// <c>join t in Teams on new { s.ContestId, s.TeamId } equals new { t.ContestId, t.TeamId }</c><br />
        /// <c>into tt from t in tt.DefaultIfEmpty()</c><br />
        /// <c>select new SolutionAuthor(s.Id, s.ContestId, s.TeamId, u.UserName, t.TeamName);</c>
        /// </para>
        /// </remarks>
        public abstract Task<List<SolutionAuthor>> FetchSolutionAuthorAsync(TContext context, Expression<Func<Submission, bool>> predicate);

        /// <summary>
        /// Create a task to fetch permitted users of one problem.
        /// </summary>
        /// <param name="context">The query database context.</param>
        /// <param name="probid">The problem ID.</param>
        /// <returns>The user information list.</returns>
        public abstract Task<IEnumerable<(int UserId, string UserName, AuthorLevel Level)>> FetchPermittedUserAsync(TContext context, int probid);

        /// <summary>
        /// Dequeue the next judging.
        /// </summary>
        /// <param name="context">The query database context.</param>
        /// <param name="judgehostName">The judgehost name.</param>
        /// <param name="extraCondition">The extra condition to attach.</param>
        /// <returns>The next judging ID or null.</returns>
        public virtual async Task<int?> DequeueNextJudgingAsync(TContext context, string judgehostName, Expression<Func<Judging, bool>>? extraCondition = null)
        {
            using var guard = await _judgementDequeueGuard.LockAsync();
            return await DequeueWithoutConcurrencyGuardAsync(context, judgehostName, extraCondition);
        }

        /// <summary>
        /// Process dequeue with non-atomic SQL operations.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="judgehostName">The name of judgehost.</param>
        /// <param name="extraCondition">The extra condition to filter.</param>
        /// <returns>The next judging ID or null.</returns>
        protected async Task<int?> DequeueWithoutConcurrencyGuardAsync(TContext context, string judgehostName, Expression<Func<Judging, bool>>? extraCondition = null)
        {
            var r = await context.GetPendingQueryable(extraCondition)
                .Select(j => new { j.Id })
                .FirstOrDefaultAsync();

            if (r == null) return null;

            DateTimeOffset now = DateTimeOffset.Now;
            await context.Judgings
                .Where(j => j.Id == r.Id)
                .BatchUpdateAsync(j => new Judging
                {
                    Status = Verdict.Running,
                    Server = judgehostName,
                    StartTime = now,
                });

            return r.Id;
        }

        /// <summary>
        /// Process dequeue with one atomic SQL operation.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="judgehostName">The name of judgehost.</param>
        /// <param name="extraCondition">The extra condition to filter.</param>
        /// <returns>The next judging ID or null.</returns>
        protected async Task<int?> DequeueWithServerSideQueryAsync(TContext context, string judgehostName, Expression<Func<Judging, bool>>? extraCondition = null)
        {
            if (context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                // In-memory provider doesn't support SQL sentences.
                using var guard = await _judgementDequeueGuard.LockAsync();
                return await DequeueWithoutConcurrencyGuardAsync(context, judgehostName, extraCondition);
            }
            else if (extraCondition != null)
            {
                throw new NotImplementedException("This is upcoming feature, currently not supported.");
            }
            else if (context.Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                DateTimeOffset now = DateTimeOffset.Now;
                FormattableString sql =
$@"UPDATE TOP(1) [p]
SET [p].[Server] = {judgehostName}, [p].[StartTime] = {now}, [p].[Status] = {(int)Verdict.Running}
OUTPUT INSERTED.[JudgingId] AS [Id]
FROM [PolygonJudgings] AS [p]
INNER JOIN [PolygonSubmissions] AS [p0] ON [p].[SubmissionId] = [p0].[SubmissionId]
INNER JOIN [PolygonLanguages] AS [p1] ON [p0].[Language] = [p1].[LangId]
INNER JOIN [PolygonProblems] AS [p2] ON [p0].[ProblemId] = [p2].[ProblemId]
WHERE ((([p].[Status] = 8) AND ([p1].[AllowJudge] = CAST(1 AS bit))) AND ([p2].[AllowJudge] = CAST(1 AS bit))) AND ([p0].[Ignored] = CAST(0 AS bit))";

                var results = await context.Set<SingleEntry>()
                    .FromSqlInterpolated(sql)
                    .ToListAsync();

                return results.SingleOrDefault()?.Id;
            }
            else if (context.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                /*
                    UPDATE server_info
                    SET    status = 'active' 
                    WHERE  server_ip = (
                        SELECT server_ip
                        FROM   server_info
                        WHERE  status = 'standby'
                        LIMIT  1
                        FOR    UPDATE SKIP LOCKED
                    )
                    RETURNING server_ip;
                */

                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException("This database provider hasn't been supported.");
            }
        }



        #region Judgehost Load Query

        private static Expression<Func<TContext, IQueryable<JudgehostLoad>>> CreateJudgehostLoadQuery(
            Expression<Func<DateTimeOffset, DateTimeOffset, double>> source)
        {
            Expression<Func<TContext, Func<DateTimeOffset, DateTimeOffset, double>, IQueryable<JudgehostLoad>>>
                superLoader = (context, diff) =>
                (
                    from j in context.Judgings
                    where j.Server != null && (j.StopTime > DateTimeOffset.Now.AddDays(-2) || j.StopTime == null)
                    group diff(j.StartTime ?? DateTimeOffset.Now.AddDays(-2), j.StopTime ?? DateTimeOffset.Now) by j.Server into g
                    select new JudgehostLoad { HostName = g.Key, Load = g.Sum(), Type = 3 }
                ).Concat(
                    from j in context.Judgings
                    where j.Server != null && (j.StopTime > DateTimeOffset.Now.AddHours(-2) || j.StopTime == null)
                    group diff(j.StartTime ?? DateTimeOffset.Now.AddHours(-2), j.StopTime ?? DateTimeOffset.Now) by j.Server into g
                    select new JudgehostLoad { HostName = g.Key, Load = g.Sum(), Type = 2 }
                ).Concat(
                    from j in context.Judgings
                    where j.Server != null && (j.StopTime > DateTimeOffset.Now.AddMinutes(-5) || j.StopTime == null)
                    group diff(j.StartTime ?? DateTimeOffset.Now.AddMinutes(-5), j.StopTime ?? DateTimeOffset.Now) by j.Server into g
                    select new JudgehostLoad { HostName = g.Key, Load = g.Sum(), Type = 1 }
                );

            return Expression.Lambda<Func<TContext, IQueryable<JudgehostLoad>>>(
                new DiffRewriter(superLoader.Parameters[1], source).Visit(superLoader.Body),
                superLoader.Parameters[0]);
        }

        private class DiffRewriter : ExpressionVisitor
        {
            private readonly ParameterExpression _diff;
            private readonly ParameterExpression _innerStart;
            private readonly ParameterExpression _innerEnd;
            private readonly Expression _innerBody;
            private Expression? _lhs = null, _rhs = null;

            public DiffRewriter(
                ParameterExpression diff,
                Expression<Func<DateTimeOffset, DateTimeOffset, double>> reload)
            {
                _diff = diff;
                _innerStart = reload.Parameters[0];
                _innerEnd = reload.Parameters[1];
                _innerBody = reload.Body;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node == _innerStart)
                    return _lhs ?? throw new ArgumentNullException(nameof(_lhs), "Where is left hand side?");
                if (node == _innerEnd)
                    return _rhs ?? throw new ArgumentNullException(nameof(_rhs), "Where is right hand side?");
                return base.VisitParameter(node);
            }

            protected override Expression VisitInvocation(InvocationExpression node)
            {
                if (node.Expression == _diff)
                {
                    _lhs = node.Arguments[0];
                    _rhs = node.Arguments[1];
                    var result = base.Visit(_innerBody);
                    _rhs = null;
                    _lhs = null;
                    return result;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        #endregion
    }
}
