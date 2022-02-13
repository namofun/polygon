using Microsoft.Extensions.DependencyInjection;
using Polygon.Packaging;
using System;
using System.Collections.Generic;

namespace Polygon
{
    /// <summary>
    /// The options class for polygon.
    /// </summary>
    public class PolygonOptions
    {
        private readonly List<(string, string, Func<IServiceProvider, IImportProvider>)> _imports = new();
        private readonly List<(string, string)> _importNames = new();
        private readonly Dictionary<string, Func<IServiceProvider, IImportProvider>> _importFactories = new();
        private bool _readonly;

        /// <summary>
        /// Ensure the value of <see cref="_readonly"/> is not true.
        /// </summary>
        private void EnsureNotReadonly()
        {
            if (_readonly) throw new InvalidOperationException("Polygon options is readonly.");
        }

        /// <summary>
        /// Finalize the settings.
        /// </summary>
        public void FinalizeSettings()
        {
            EnsureNotReadonly();
            _readonly = true;

            foreach (var (id, name, factory) in _imports)
            {
                _importNames.Add((id, name));
                _importFactories.Add(id, factory);
            }
        }

        /// <summary>
        /// Add an import provider to options.
        /// </summary>
        /// <typeparam name="T">The provider implementation.</typeparam>
        /// <param name="id">The id of provider.</param>
        /// <param name="name">The name of provider.</param>
        /// <returns>The options to chain changes.</returns>
        public PolygonOptions AddImportProvider<T>(string id, string name)
            where T : class, IImportProvider
        {
            EnsureNotReadonly();
            _imports.Add((id, name, s => s.GetRequiredService<T>()));
            return this;
        }

        /// <summary>
        /// The name of import providers
        /// </summary>
        public IReadOnlyCollection<(string, string)> ImportProviders => _importNames;

        /// <summary>
        /// Create an instance of import providers.
        /// </summary>
        /// <param name="services">The service provider.</param>
        /// <param name="id">The provider ID.</param>
        /// <returns>The import provider if found.</returns>
        public IImportProvider? CreateImportProviders(IServiceProvider services, string id)
        {
            if (!_importFactories.TryGetValue(id, out var factory))
                return null;
            return factory.Invoke(services);
        }
    }
}
