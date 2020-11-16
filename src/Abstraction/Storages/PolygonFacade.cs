namespace Polygon.Storages
{
    /// <summary>
    /// The big facade interface.
    /// </summary>
    public interface IPolygonFacade
    {
        /// <inheritdoc cref="IProblemStore" />
        IProblemStore Problems { get; }

        /// <inheritdoc cref="ITestcaseStore" />
        ITestcaseStore Testcases { get; }

        /// <inheritdoc cref="ISubmissionStore" />
        ISubmissionStore Submissions { get; }

        /// <inheritdoc cref="IExecutableStore" />
        IExecutableStore Executables { get; }

        /// <inheritdoc cref="IInternalErrorStore" />
        IInternalErrorStore InternalErrors { get; }

        /// <inheritdoc cref="IJudgehostStore" />
        IJudgehostStore Judgehosts { get; }

        /// <inheritdoc cref="IJudgingStore" />
        IJudgingStore Judgings { get; }

        /// <inheritdoc cref="ILanguageStore" />
        ILanguageStore Languages { get; }

        /// <inheritdoc cref="IRejudgingStore" />
        IRejudgingStore Rejudgings { get; }
    }

    /// <summary>
    /// The big facade interface with one implemention.
    /// </summary>
    public interface IPolygonFacade2 :
        IPolygonFacade,
        IProblemStore,
        ITestcaseStore,
        ISubmissionStore,
        IExecutableStore,
        IInternalErrorStore,
        IJudgehostStore,
        IJudgingStore,
        ILanguageStore,
        IRejudgingStore
    {
    }

    /// <inheritdoc cref="IPolygonFacade" />
    internal class CompositePolygonFacade : IPolygonFacade
    {
        /// <inheritdoc />
        public IProblemStore Problems { get; }

        /// <inheritdoc />
        public ITestcaseStore Testcases { get; }

        /// <inheritdoc />
        public ISubmissionStore Submissions { get; }

        /// <inheritdoc />
        public IExecutableStore Executables { get; }

        /// <inheritdoc />
        public IInternalErrorStore InternalErrors { get; }

        /// <inheritdoc />
        public IJudgehostStore Judgehosts { get; }

        /// <inheritdoc />
        public IJudgingStore Judgings { get; }

        /// <inheritdoc />
        public ILanguageStore Languages { get; }

        /// <inheritdoc />
        public IRejudgingStore Rejudgings { get; }

        public CompositePolygonFacade(
            IProblemStore a,
            ITestcaseStore b,
            ISubmissionStore c,
            IExecutableStore d,
            IInternalErrorStore e,
            IJudgehostStore f,
            IJudgingStore g,
            ILanguageStore h,
            IRejudgingStore i)
        {
            Problems = a;
            Testcases = b;
            Submissions = c;
            Executables = d;
            InternalErrors = e;
            Judgehosts = f;
            Judgings = g;
            Languages = h;
            Rejudgings = i;
        }
    }
}
