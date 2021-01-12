using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
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

        Task ILanguageStore.UpdateAsync(Language entity) => UpdateEntityAsync(entity);

        Task ILanguageStore.UpdateAsync(string id, Expression<Func<Language, Language>> expression)
        {
            return Context.Languages.Where(l => l.Id == id).BatchUpdateAsync(expression);
        }

        Task<Language> ILanguageStore.FindAsync(string langid)
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
