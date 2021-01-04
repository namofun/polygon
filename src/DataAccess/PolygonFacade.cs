using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    /// <summary>
    /// The big facade interface with one implemention.
    /// </summary>
    /// <typeparam name="TContext">The context type.</typeparam>
    /// <typeparam name="TQueryCache">The query cache type.</typeparam>
    public partial class PolygonFacade<TContext, TQueryCache> : IPolygonFacade
        where TContext : DbContext
        where TQueryCache : QueryCacheBase<TContext>
    {
        /// <inheritdoc />
        IProblemStore IPolygonFacade.Problems => this;

        /// <inheritdoc />
        ITestcaseStore IPolygonFacade.Testcases => this;

        /// <inheritdoc />
        ISubmissionStore IPolygonFacade.Submissions => this;

        /// <inheritdoc />
        IExecutableStore IPolygonFacade.Executables => this;

        /// <inheritdoc />
        IInternalErrorStore IPolygonFacade.InternalErrors => this;

        /// <inheritdoc />
        IJudgehostStore IPolygonFacade.Judgehosts => this;

        /// <inheritdoc />
        IJudgingStore IPolygonFacade.Judgings => this;

        /// <inheritdoc />
        ILanguageStore IPolygonFacade.Languages => this;

        /// <inheritdoc />
        IRejudgingStore IPolygonFacade.Rejudgings => this;

        public TContext Context { get; }

        public IJudgingFileProvider JudgingFiles { get; }

        public IProblemFileProvider ProblemFiles { get; }

        public IMediator Mediator { get; }

        public TQueryCache QueryCache { get; }

        public PolygonFacade(TContext context, IJudgingFileProvider jf, IProblemFileProvider pf, IMediator mediator, TQueryCache queryCache)
        {
            Context = context;
            JudgingFiles = jf;
            ProblemFiles = pf;
            Mediator = mediator;
            QueryCache = queryCache;
        }

        private async Task<T> CreateEntityAsync<T>(T entity) where T : class
        {
            var e = Context.Set<T>().Add(entity);
            await Context.SaveChangesAsync();
            return e.Entity;
        }

        private Task DeleteEntityAsync<T>(T entity) where T : class
        {
            Context.Set<T>().Remove(entity);
            return Context.SaveChangesAsync();
        }

        private Task UpdateEntityAsync<T>(T entity) where T : class
        {
            Context.Set<T>().Update(entity);
            return Context.SaveChangesAsync();
        }
    }
}
