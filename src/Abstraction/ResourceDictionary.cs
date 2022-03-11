using Microsoft.Extensions.DependencyInjection;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Packaging;

namespace Xylab.Polygon
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

        /// <summary>
        /// Converts the verdict to single char.
        /// </summary>
        /// <param name="verdict">The raw verdict.</param>
        /// <returns>The char to represent.</returns>
        public static char ConvertToChar(Verdict? verdict)
        {
            return verdict switch
            {
                Verdict.Accepted => 'a',
                Verdict.MemoryLimitExceeded => 'm',
                Verdict.OutputLimitExceeded => 'o',
                Verdict.RuntimeError => 'r',
                Verdict.TimeLimitExceeded => 't',
                Verdict.UndefinedError => 'u',
                Verdict.WrongAnswer => 'w',
                _ => '?',
            };
        }

        /// <summary>
        /// Converts the single char to verdict.
        /// </summary>
        /// <param name="ch">The single char.</param>
        /// <returns>The char to represent.</returns>
        public static Verdict ConvertToVerdict(char ch)
        {
            return ch switch
            {
                'a' => Verdict.Accepted,
                'm' => Verdict.MemoryLimitExceeded,
                'o' => Verdict.OutputLimitExceeded,
                'r' => Verdict.RuntimeError,
                't' => Verdict.TimeLimitExceeded,
                'u' => Verdict.UndefinedError,
                'w' => Verdict.WrongAnswer,
                _ => Verdict.Unknown,
            };
        }
    }
}
