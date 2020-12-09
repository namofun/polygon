using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Polygon.Entities;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    public partial class PolygonFacade<TUser, TContext> : IJudgehostStore
    {
        DbSet<Judgehost> Judgehosts => Context.Set<Judgehost>();

        Task<Judgehost> IJudgehostStore.CreateAsync(Judgehost entity) => CreateEntityAsync(entity);

        Task<Judgehost> IJudgehostStore.FindAsync(string name)
        {
            return Judgehosts
                .Where(h => h.ServerName == name)
                .SingleOrDefaultAsync();
        }

        Task<IPagedList<Judging>> IJudgehostStore.FetchJudgingsAsync(string hostname, int page, int count)
        {
            return Judgings
                .Where(j => j.Server == hostname)
                .OrderByDescending(g => g.Id)
                .ToPagedListAsync(page, count);
        }

        Task<int> IJudgehostStore.CountFailureAsync()
        {
            return Judgehosts
                .Where(jh => jh.PollTime < DateTimeOffset.Now.AddSeconds(-120) && jh.Active)
                .CountAsync();
        }

        Task<List<Judgehost>> IJudgehostStore.ListAsync()
        {
            return Judgehosts.ToListAsync();
        }

        Task IJudgehostStore.NotifyPollAsync(Judgehost host)
        {
            host.PollTime = DateTimeOffset.Now;
            return UpdateEntityAsync(host);
        }

        Task<int> IJudgehostStore.ToggleAsync(string? hostname, bool active)
        {
            return Judgehosts
                .WhereIf(hostname != null, h => h.ServerName == hostname)
                .BatchUpdateAsync(h => new Judgehost { Active = active });
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
                    return _lhs ?? throw new ArgumentNullException("Where is left hand side?");
                if (node == _innerEnd)
                    return _rhs ?? throw new ArgumentNullException("Where is right hand side?");
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

        private static Expression<Func<TContext, IQueryable<JudgehostLoad>>> CreateQuery(
            Expression<Func<DateTimeOffset, DateTimeOffset, double>> source)
        {
            Expression<Func<TContext, Func<DateTimeOffset, DateTimeOffset, double>, IQueryable<JudgehostLoad>>>
                superLoader = (context, diff) =>
                (
                    from j in context.Set<Judging>()
                    where j.Server != null && (j.StopTime > DateTimeOffset.Now.AddDays(-2) || j.StopTime == null)
                    group diff(j.StartTime ?? DateTimeOffset.Now.AddDays(-2), j.StopTime ?? DateTimeOffset.Now) by j.Server into g
                    select new JudgehostLoad { HostName = g.Key, Load = g.Sum(), Type = 3 }
                ).Concat(
                    from j in context.Set<Judging>()
                    where j.Server != null && (j.StopTime > DateTimeOffset.Now.AddHours(-2) || j.StopTime == null)
                    group diff(j.StartTime ?? DateTimeOffset.Now.AddHours(-2), j.StopTime ?? DateTimeOffset.Now) by j.Server into g
                    select new JudgehostLoad { HostName = g.Key, Load = g.Sum(), Type = 2 }
                ).Concat(
                    from j in context.Set<Judging>()
                    where j.Server != null && (j.StopTime > DateTimeOffset.Now.AddMinutes(-5) || j.StopTime == null)
                    group diff(j.StartTime ?? DateTimeOffset.Now.AddMinutes(-5), j.StopTime ?? DateTimeOffset.Now) by j.Server into g
                    select new JudgehostLoad { HostName = g.Key, Load = g.Sum(), Type = 1 }
                );

            return Expression.Lambda<Func<TContext, IQueryable<JudgehostLoad>>>(
                new DiffRewriter(superLoader.Parameters[1], source).Visit(superLoader.Body),
                superLoader.Parameters[0]);
        }

        /// <remarks>
        /// For original query, please refer to <see cref="CreateQuery(Expression{Func{DateTimeOffset, DateTimeOffset, double}})"/>.
        /// </remarks>
        Task<Dictionary<string, (double, double, double)>> IJudgehostStore.LoadAsync()
        {
            var compiledQuery = CachedQueryable.Cache.GetOrCreate(
                key: "judgehost_loads_query",
                factory: entry => EF.CompileAsyncQuery(CreateQuery(Context.CalculateDuration)));

            return CachedQueryable.Cache.GetOrCreateAsync("judgehost_loads", async entry =>
            {
                var returns = new Dictionary<string, (double, double, double)>();

                await foreach (var item in compiledQuery(Context))
                {
                    if (!returns.ContainsKey(item.HostName)) returns.Add(item.HostName, default);
                    var current = returns[item.HostName];
                    if (item.Type == 1) current.Item1 = item.Load / 5.0;
                    else if (item.Type == 2) current.Item2 = item.Load / (2.0 * 60);
                    else if (item.Type == 3) current.Item3 = item.Load / (2.0 * 24 * 60);
                    returns[item.HostName] = current;
                }

                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return returns;
            });
        }
    }

    /// <summary>
    /// The expression replacing visitor.
    /// </summary>
    internal class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly Dictionary<Expression, Expression> _changes = new Dictionary<Expression, Expression>();

        /// <summary>
        /// Attach the expression to change from <paramref name="from"/> to <paramref name="to"/>.
        /// </summary>
        /// <param name="from">The source expression.</param>
        /// <param name="to">The target expression.</param>
        /// <returns>The resulting visitor.</returns>
        public ReplaceExpressionVisitor Attach(Expression from, Expression to)
        {
            _changes.Add(from, to);
            return this;
        }

        /// <inheritdoc />
        public override Expression Visit(Expression node)
        {
            return node == null ? null! : _changes.TryGetValue(node, out var exp) ? exp : base.Visit(node);
        }
    }
}
