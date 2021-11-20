using Microsoft.Extensions.DependencyInjection;
using System;

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

    /// <inheritdoc cref="IPolygonFacade" />
    internal class CompositePolygonFacade : IPolygonFacade
    {
        private readonly Lazy<IProblemStore> _problems;
        private readonly Lazy<ITestcaseStore> _testcases;
        private readonly Lazy<ISubmissionStore> _submissions;
        private readonly Lazy<IExecutableStore> _executables;
        private readonly Lazy<IInternalErrorStore> _errors;
        private readonly Lazy<IJudgehostStore> _judgehosts;
        private readonly Lazy<IJudgingStore> _judgings;
        private readonly Lazy<ILanguageStore> _languages;
        private readonly Lazy<IRejudgingStore> _rejudgings;

        public IProblemStore Problems => _problems.Value;
        public ITestcaseStore Testcases => _testcases.Value;
        public ISubmissionStore Submissions => _submissions.Value;
        public IExecutableStore Executables => _executables.Value;
        public IInternalErrorStore InternalErrors => _errors.Value;
        public IJudgehostStore Judgehosts => _judgehosts.Value;
        public IJudgingStore Judgings => _judgings.Value;
        public ILanguageStore Languages => _languages.Value;
        public IRejudgingStore Rejudgings => _rejudgings.Value;

        public CompositePolygonFacade(IServiceProvider serviceProvider)
        {
            Lazy<T> Resolve<T>() where T : class
                => new(serviceProvider.GetRequiredService<T>);

            _rejudgings = Resolve<IRejudgingStore>();
            _submissions = Resolve<ISubmissionStore>();
            _testcases = Resolve<ITestcaseStore>();
            _problems = Resolve<IProblemStore>();
            _languages = Resolve<ILanguageStore>();
            _judgings = Resolve<IJudgingStore>();
            _judgehosts = Resolve<IJudgehostStore>();
            _executables = Resolve<IExecutableStore>();
            _errors = Resolve<IInternalErrorStore>();
        }
    }
}
