using Microsoft.Extensions.DependencyInjection;
using Polygon.Packaging;

namespace Polygon
{
    /// <summary>
    /// The static resource dictionary.
    /// </summary>
    public static class ResourceDictionary
    {
        /// <summary>
        /// The available markdown files
        /// </summary>
        public static readonly string[] MarkdownFiles = new[]
        {
            "description",
            "inputdesc",
            "outputdesc",
            "hint",
            "interact"
        };

        /// <summary>
        /// The menu for polygon navbar
        /// </summary>
        public const string MenuNavbar = "Menu_PolygonNavbar";

        /// <summary>
        /// The components for problem overview
        /// </summary>
        public const string ComponentProblemOverview = "Component_Polygon_ProblemOverview";

        /// <summary>
        /// Add import provider to polygon.
        /// </summary>
        /// <typeparam name="T">The import service type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="id">The provider ID.</param>
        /// <param name="name">The provider name.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddImportProvider<T>(this IServiceCollection services, string id, string name)
            where T : class, IImportProvider
        {
            services.AddScoped<T>();
            services.Configure<PolygonOptions>(o => o.AddImportProvider<T>(id, name));
            return services;
        }
    }
}
