using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoadQuery = System.Linq.IQueryable<Polygon.Models.JudgehostLoad>;
using Timer = System.Linq.Expressions.Expression<System.Func<System.DateTimeOffset>>;

namespace Polygon.Storages
{
    public partial class PolygonFacade<TUser, TRole, TContext> : IJudgehostStore
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

        /// <remarks>
        /// <para>
        /// Original Query:
        /// </para>
        /// <para>
        /// <c>from j in Judgings</c><br />
        /// <c>where j.Server != null &amp;&amp; (j.StopTime > DateTimeOffset.Now.AddDays(-2) || j.StopTime == null)</c><br />
        /// <c>group EF.Functions.DateDiffSecond(j.StartTime ?? DateTimeOffset.Now.AddDays(-2), j.StopTime ?? DateTimeOffset.Now) by j.Server into g</c><br />
        /// <c>select new { HostName = g.Key, Load = g.Sum(), Type = 3 };</c>
        /// </para>
        /// <para>
        /// <c>from j in Judgings</c><br />
        /// <c>where j.Server != null &amp;&amp; (j.StopTime > DateTimeOffset.Now.AddHours(-2) || j.StopTime == null)</c><br />
        /// <c>group EF.Functions.DateDiffSecond(j.StartTime ?? DateTimeOffset.Now.AddHours(-2), j.StopTime ?? DateTimeOffset.Now) by j.Server into g</c><br />
        /// <c>select new { HostName = g.Key, Load = g.Sum(), Type = 2 };</c>
        /// </para>
        /// <para>
        /// <c>from j in Judgings</c><br />
        /// <c>where j.Server != null &amp;&amp; (j.StopTime > DateTimeOffset.Now.AddMinutes(-5) || j.StopTime == null)</c><br />
        /// <c>group EF.Functions.DateDiffSecond(j.StartTime ?? DateTimeOffset.Now.AddMinutes(-5), j.StopTime ?? DateTimeOffset.Now) by j.Server into g</c><br />
        /// <c>select new { HostName = g.Key, Load = g.Sum(), Type = 1 };</c>
        /// </para>
        /// <para>
        /// <c>query1.Concat(query2).Concat(query3).ToList()</c>
        /// </para>
        /// </remarks>
        Task<Dictionary<string, (double, double, double)>> IJudgehostStore.LoadAsync()
        {
            var compiledQuery = CachedQueryable.Cache.GetOrCreate("judgehost_loads_query", entry =>
            {
                var originalQuery = Context.JudgehostLoadQuery;
                Expression<Func<TContext, DbSet<Judging>>> j = context => context.Set<Judging>();

                var query1 = new ReplaceExpressionVisitor()
                    .Attach(originalQuery.Parameters[0], j.Body)
                    .Attach(originalQuery.Parameters[1], Expression.Constant(3, typeof(int)))
                    .Attach(originalQuery.Parameters[2], ((Timer)(() => DateTimeOffset.Now.AddDays(-2))).Body)
                    .Visit(originalQuery.Body);

                var query2 = new ReplaceExpressionVisitor()
                    .Attach(originalQuery.Parameters[0], j.Body)
                    .Attach(originalQuery.Parameters[1], Expression.Constant(2, typeof(int)))
                    .Attach(originalQuery.Parameters[2], ((Timer)(() => DateTimeOffset.Now.AddHours(-2))).Body)
                    .Visit(originalQuery.Body);

                var query3 = new ReplaceExpressionVisitor()
                    .Attach(originalQuery.Parameters[0], j.Body)
                    .Attach(originalQuery.Parameters[1], Expression.Constant(1, typeof(int)))
                    .Attach(originalQuery.Parameters[2], ((Timer)(() => DateTimeOffset.Now.AddMinutes(-5))).Body)
                    .Visit(originalQuery.Body);

                Expression<Func<LoadQuery, LoadQuery, LoadQuery, LoadQuery>> concatToList =
                    (query1, query2, query3) => query1.Concat(query2).Concat(query3);

                var finalQuery = new ReplaceExpressionVisitor()
                    .Attach(concatToList.Parameters[0], query1)
                    .Attach(concatToList.Parameters[1], query2)
                    .Attach(concatToList.Parameters[2], query3)
                    .Visit(concatToList.Body);

                var exp = Expression.Lambda<Func<TContext, LoadQuery>>(finalQuery, j.Parameters);
                return EF.CompileAsyncQuery(exp);
            });

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
