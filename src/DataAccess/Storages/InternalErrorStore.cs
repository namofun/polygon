using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    public partial class PolygonFacade<TContext, TQueryCache> : IInternalErrorStore
    {
        Task<InternalError> IInternalErrorStore.CreateAsync(InternalError entity) => CreateEntityAsync(entity);

        Task<int> IInternalErrorStore.CountOpenAsync()
        {
            return Context.InternalErrors
                .Where(ie => ie.Status == InternalErrorStatus.Open)
                .CountAsync();
        }

        Task<InternalError?> IInternalErrorStore.FindAsync(int id)
        {
            return Context.InternalErrors
                .Where(e => e.Id == id)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        Task<IPagedList<InternalError>> IInternalErrorStore.ListAsync(int page, int count)
        {
            return Context.InternalErrors
                .OrderByDescending(e => e.Id)
                .Select(e => new InternalError(e.Id, e.Status, e.Time, e.Description))
                .ToPagedListAsync(page, count);
        }

        async Task<InternalErrorDisable?> IInternalErrorStore.ResolveAsync(InternalError error, InternalErrorStatus status)
        {
            if (error.Status != InternalErrorStatus.Open || status == InternalErrorStatus.Open)
                throw new InvalidOperationException();

            error.Status = status;
            await Context.InternalErrors
                .Where(e => e.Id == error.Id)
                .BatchUpdateAsync(e => new InternalError { Status = status });

            if (status != InternalErrorStatus.Resolved) return null;
            return error.Disabled.AsJson<InternalErrorDisable>();
        }
    }
}
