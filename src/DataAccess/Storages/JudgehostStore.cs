using Microsoft.EntityFrameworkCore;
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
    }
}
