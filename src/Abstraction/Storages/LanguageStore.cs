using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon.Storages
{
    /// <summary>
    /// The store interface for <see cref="Language"/>.
    /// </summary>
    public interface ILanguageStore
    {
        /// <summary>
        /// Create an instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The created entity.</returns>
        Task<Language> CreateAsync(Language entity);

        /// <summary>
        /// Update the instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="expression">The update expression.</param>
        /// <returns>The update task.</returns>
        Task UpdateAsync(Language entity, Expression<Func<Language, Language>> expression);

        /// <summary>
        /// Delete the instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The delete task.</returns>
        Task DeleteAsync(Language entity);

        /// <summary>
        /// Find the entity.
        /// </summary>
        /// <param name="langid">The entity id.</param>
        /// <returns>The find task.</returns>
        Task<Language?> FindAsync(string langid);

        /// <summary>
        /// Toggle the allow judge flag for language.
        /// </summary>
        /// <param name="langid">The language ID.</param>
        /// <param name="tobe">The toggle result.</param>
        /// <returns>The toggle task.</returns>
        Task ToggleJudgeAsync(string langid, bool tobe);

        /// <summary>
        /// Toggle the allow submit flag for language.
        /// </summary>
        /// <param name="langid">The language ID.</param>
        /// <param name="tobe">The toggle result.</param>
        /// <returns>The toggle task.</returns>
        Task ToggleSubmitAsync(string langid, bool tobe);

        /// <summary>
        /// List the language by types.
        /// </summary>
        /// <param name="active">The active state of listed entites.</param>
        /// <returns>The language list.</returns>
        Task<List<Language>> ListAsync(bool? active = null);
    }
}
