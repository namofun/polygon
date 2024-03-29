﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon.Storages
{
    public partial class PolygonFacade<TContext, TQueryCache> : IJudgehostStore
    {
        Task<Judgehost> IJudgehostStore.CreateAsync(Judgehost entity) => CreateEntityAsync(entity);

        Task<Judgehost?> IJudgehostStore.FindAsync(string name)
        {
            return Context.Judgehosts
                .AsNoTracking()
                .Where(h => h.ServerName == name)
                .SingleOrDefaultAsync();
        }

        Task<IPagedList<Judging>> IJudgehostStore.FetchJudgingsAsync(string hostname, int page, int count)
        {
            return Context.Judgings
                .Where(j => j.Server == hostname)
                .OrderByDescending(g => g.Id)
                .ToPagedListAsync(page, count);
        }

        Task<int> IJudgehostStore.CountFailureAsync()
        {
            return Context.Judgehosts
                .Where(jh => jh.PollTime < DateTimeOffset.Now.AddSeconds(-120) && jh.Active)
                .CountAsync();
        }

        Task<List<Judgehost>> IJudgehostStore.ListAsync()
        {
            return Context.Judgehosts.ToListAsync();
        }

        Task IJudgehostStore.NotifyPollAsync(Judgehost host)
        {
            host.PollTime = DateTimeOffset.Now;
            return Context.Judgehosts
                .Where(h => h.ServerName == host.ServerName)
                .BatchUpdateAsync(h => new Judgehost { PollTime = host.PollTime });
        }

        Task<int> IJudgehostStore.ToggleAsync(string? hostname, bool active)
        {
            return Context.Judgehosts
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
                if (item.Type == 1) current.Item1 = item.Load / (5.0 * 60);
                else if (item.Type == 2) current.Item2 = item.Load / (2.0 * 60 * 60);
                else if (item.Type == 3) current.Item3 = item.Load / (2.0 * 24 * 60 * 60);
                returns[item.HostName] = current;
            }

            return returns;
        }
    }
}
