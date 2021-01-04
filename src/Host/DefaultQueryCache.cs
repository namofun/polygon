﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using Polygon.Models;
using Polygon.Storages;
using SatelliteSite.IdentityModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SatelliteSite
{
    public class QueryCache<TContext> : QueryCacheBase<TContext>
        where TContext : IdentityDbContext<User, Role, int>
    {
        public override Task<List<SolutionAuthor>> FetchSolutionAuthorAsync(TContext context, Expression<Func<Submission, bool>> predicate)
        {
            var query =
                from s in context.Set<Submission>().WhereIf(predicate != null, predicate)
                join u in context.Users on new { s.ContestId, s.TeamId } equals new { ContestId = 0, TeamId = u.Id }
                into uu from u in uu.DefaultIfEmpty()
                // join t in Set<Team>() on new { s.ContestId, s.Author } equals new { t.ContestId, Author = t.TeamId }
                // into tt from t in tt.DefaultIfEmpty()
                select new SolutionAuthor(s.Id, s.ContestId, s.TeamId, u.UserName, null /*t.TeamName */);
            return query.ToListAsync();
        }

        public override async Task<IEnumerable<(int UserId, string UserName, string NickName)>> FetchPermittedUserAsync(TContext context, int probid)
        {
            var query =
                from pa in context.Set<ProblemAuthor>()
                where pa.ProblemId == probid
                join u in context.Users on pa.UserId equals u.Id
                select new { u.Id, u.UserName, u.NickName };
            return (await query.ToListAsync()).Select(a => (a.Id, a.UserName, a.NickName));
        }

        protected override Expression<Func<DateTimeOffset, DateTimeOffset, double>> CalculateDuration { get; }
            = (start, end) => EF.Functions.DateDiffMillisecond(start, end) / 1000.0;
    }
}
