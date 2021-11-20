using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using Polygon.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    public partial class PolygonFacade<TContext, TQueryCache> : ILanguageStore
    {
        Task<Language> ILanguageStore.CreateAsync(Language entity) => CreateEntityAsync(entity);

        Task ILanguageStore.DeleteAsync(Language entity) => DeleteEntityAsync(entity);

        async Task ILanguageStore.UpdateAsync(Language entity, Expression<Func<Language, Language>> expression)
        {
            string id = entity.Id;

            await Context.Languages
                .Where(l => l.Id == id)
                .BatchUpdateAsync(expression);

            await Context.Entry(entity).ReloadAsync();
            await Mediator.Publish(new LanguageModifiedEvent(entity));
        }

        Task<Language?> ILanguageStore.FindAsync(string langid)
        {
            return Context.Languages
                .Where(l => l.Id == langid)
                .SingleOrDefaultAsync();
        }

        Task<List<Language>> ILanguageStore.ListAsync(bool? active)
        {
            return Context.Languages
                .WhereIf(active.HasValue, l => l.AllowSubmit == active!)
                .ToListAsync();
        }

        Task ILanguageStore.ToggleJudgeAsync(string langid, bool tobe)
        {
            return Context.Languages
                .Where(l => l.Id == langid)
                .BatchUpdateAsync(l => new Language { AllowJudge = tobe });
        }

        Task ILanguageStore.ToggleSubmitAsync(string langid, bool tobe)
        {
            return Context.Languages
                .Where(l => l.Id == langid)
                .BatchUpdateAsync(l => new Language { AllowSubmit = tobe });
        }
    }
}
