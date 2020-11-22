using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    public partial class PolygonFacade<TUser, TRole, TContext> : IInternalErrorStore
    {
        public DbSet<InternalError> InternalErrors => Context.Set<InternalError>();

        Task<InternalError> IInternalErrorStore.CreateAsync(InternalError entity) => CreateEntityAsync(entity);

        Task IInternalErrorStore.UpdateAsync(InternalError entity) => UpdateEntityAsync(entity);

        Task IInternalErrorStore.UpdateAsync(int id, Expression<Func<InternalError, InternalError>> expression)
        {
            return InternalErrors
                .Where(e => e.Id == id)
                .BatchUpdateAsync(expression);
        }

        Task<int> IInternalErrorStore.CountOpenAsync()
        {
            return InternalErrors
                .Where(ie => ie.Status == InternalErrorStatus.Open)
                .CountAsync();
        }

        Task<InternalError> IInternalErrorStore.FindAsync(int id)
        {
            return InternalErrors
                .Where(e => e.Id == id)
                .SingleOrDefaultAsync();
        }

        Task<IPagedList<InternalError>> IInternalErrorStore.ListAsync(int page, int count)
        {
            return InternalErrors
                .OrderByDescending(e => e.Id)
                .Select(e => new InternalError(e.Id, e.Status, e.Time, e.Description))
                .ToPagedListAsync(page, count);
        }

        async Task<InternalErrorDisable?> IInternalErrorStore.ResolveAsync(InternalError error, InternalErrorStatus status)
        {
            if (error.Status != InternalErrorStatus.Open || status == InternalErrorStatus.Open)
                throw new InvalidOperationException();
            error.Status = status;
            await UpdateEntityAsync(error);

            if (status != InternalErrorStatus.Resolved) return null;
            return error.Disabled.AsJson<InternalErrorDisable>();
        }
    }
}
