using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    /// <inheritdoc cref="IPolygonFacade2" />
    public partial class PolygonFacade<TUser, TRole, TContext> : IPolygonFacade2
        where TContext : DbContext
        where TUser : SatelliteSite.IdentityModule.Entities.User
        where TRole : SatelliteSite.IdentityModule.Entities.Role, IRoleWithProblem
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

        public PolygonFacade(TContext context, IJudgingFileProvider jf, IProblemFileProvider pf)
        {
            Context = context;
            JudgingFiles = jf;
            ProblemFiles = pf;
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
