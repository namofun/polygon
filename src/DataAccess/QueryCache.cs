using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Entities;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Polygon.Storages
{
    public class QueryCache<TContext> where TContext : DbContext, IPolygonQueryable
    {
        public Func<TContext, IAsyncEnumerable<JudgehostLoad>> JudgehostLoad { get; }

        public IServiceProvider Services { get; }

        public QueryCache(IServiceProvider serviceProvider)
        {
            Services = serviceProvider;

            using var scope = Services.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<TContext>();
            JudgehostLoad = EF.CompileAsyncQuery(CreateJudgehostLoadQuery(context.CalculateDuration));
        }

        private static Expression<Func<TContext, IQueryable<JudgehostLoad>>> CreateJudgehostLoadQuery(
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
    }
}
