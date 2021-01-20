using Polygon;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PolygonDataAccessExtensions
    {
        /// <summary>
        /// Configure the options of <see cref="PolygonPhysicalOptions"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="setupAction">The setup action of <see cref="PolygonPhysicalOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection ConfigurePolygonStorage(
            this IServiceCollection services,
            Action<PolygonPhysicalOptions> setupAction)
        {
            return services.Configure(setupAction);
        }
    }
}
