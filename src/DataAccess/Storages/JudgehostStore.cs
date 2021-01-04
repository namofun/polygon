using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    public partial class PolygonFacade<TContext, TQueryCache> : IJudgehostStore
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

        async Task<Dictionary<string, (double, double, double)>> IJudgehostStore.LoadAsync()
        {
            var returns = new Dictionary<string, (double, double, double)>();

            var loads = await QueryCache.FetchJudgehostLoadAsync(Context);
            foreach (var item in loads)
            {
                returns.TryAdd(item.HostName, default);
                var current = returns[item.HostName];
                if (item.Type == 1) current.Item1 = item.Load / 5.0;
                else if (item.Type == 2) current.Item2 = item.Load / (2.0 * 60);
                else if (item.Type == 3) current.Item3 = item.Load / (2.0 * 24 * 60);
                returns[item.HostName] = current;
            }

            return returns;
        }
    }
}
