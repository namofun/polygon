using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using Polygon.Models;
using Polygon.Storages;
using SatelliteSite.IdentityModule.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SatelliteSite
{
    public class DefaultContext : IdentityDbContext<User, AspNetRole, int>, IPolygonQueryable
    {
        public DefaultContext(DbContextOptions options)
            : base(options)
        {
        }

        public IQueryable<SolutionAuthor> Author(Expression<Func<Submission, bool>> predicate)
        {
            return
                from s in Set<Submission>().WhereIf(predicate != null, predicate)
                join u in Set<User>() on new { s.ContestId, s.TeamId } equals new { ContestId = 0, TeamId = u.Id }
                into uu from u in uu.DefaultIfEmpty()
                // join t in Set<Team>() on new { s.ContestId, s.Author } equals new { t.ContestId, Author = t.TeamId }
                // into tt from t in tt.DefaultIfEmpty()
                select new SolutionAuthor(s.Id, s.ContestId, s.TeamId, u.UserName, null /*t.TeamName */);
        }

        public Expression<Func<DbSet<Judging>, int, DateTimeOffset, IQueryable<JudgehostLoad>>> JudgehostLoadQuery { get; }
            = (Judgings, type, time) =>
                from j in Judgings
                where j.Server != null && (j.StopTime > time || j.StopTime == null)
                group EF.Functions.DateDiffSecond(j.StartTime ?? time, j.StopTime ?? DateTimeOffset.Now) by j.Server into g
                select new JudgehostLoad { HostName = g.Key, Load = g.Sum(), Type = type };
    }
}
