using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        Task<Dictionary<string, (double, double, double)>> IJudgehostStore.LoadAsync()
        {
            return CachedQueryable.Cache.GetOrCreateAsync("judgehost_loads", async entry =>
            {
                var query1 =
                    from j in Judgings
                    where j.Server != null && (j.StopTime > DateTimeOffset.Now.AddDays(-2) || j.StopTime == null)
                    group EF.Functions.DateDiffSecond(j.StartTime ?? DateTimeOffset.Now.AddDays(-2), j.StopTime ?? DateTimeOffset.Now) by j.Server into g
                    select new { HostName = g.Key, Load = g.Sum(), Type = 3 };
                
                var query2 =
                    from j in Judgings
                    where j.Server != null && (j.StopTime > DateTimeOffset.Now.AddHours(-2) || j.StopTime == null)
                    group EF.Functions.DateDiffSecond(j.StartTime ?? DateTimeOffset.Now.AddHours(-2), j.StopTime ?? DateTimeOffset.Now) by j.Server into g
                    select new { HostName = g.Key, Load = g.Sum(), Type = 2 };

                var query3 =
                    from j in Judgings
                    where j.Server != null && (j.StopTime > DateTimeOffset.Now.AddMinutes(-5) || j.StopTime == null)
                    group EF.Functions.DateDiffSecond(j.StartTime ?? DateTimeOffset.Now.AddMinutes(-5), j.StopTime ?? DateTimeOffset.Now) by j.Server into g
                    select new { HostName = g.Key, Load = g.Sum(), Type = 1 };

                var results = await query1.Concat(query2).Concat(query3).ToListAsync();
                var returns = new Dictionary<string, (double, double, double)>();

                foreach (var item in results)
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
}
