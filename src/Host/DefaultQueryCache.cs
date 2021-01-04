﻿using Microsoft.EntityFrameworkCore;
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
    public class DefaultQueryCache : QueryCacheBase<DefaultContext>
    {
        public override Task<List<SolutionAuthor>> FetchSolutionAuthorAsync(DefaultContext context, Expression<Func<Submission, bool>> predicate)
        {
            var query =
                from s in context.Set<Submission>().WhereIf(predicate != null, predicate)
                join u in context.Set<User>() on new { s.ContestId, s.TeamId } equals new { ContestId = 0, TeamId = u.Id }
                into uu from u in uu.DefaultIfEmpty()
                // join t in Set<Team>() on new { s.ContestId, s.Author } equals new { t.ContestId, Author = t.TeamId }
                // into tt from t in tt.DefaultIfEmpty()
                select new SolutionAuthor(s.Id, s.ContestId, s.TeamId, u.UserName, null /*t.TeamName */);
            return query.ToListAsync();
        }

        protected override Expression<Func<DateTimeOffset, DateTimeOffset, double>> CalculateDuration { get; }
            = (start, end) => EF.Functions.DateDiffMillisecond(start, end) / 1000.0;
    }
}
