using Microsoft.Extensions.DependencyInjection;

namespace Polygon
{
    /// <summary>
    /// Configure the service role for polygon.
    /// </summary>
    public interface IServiceRole
    {
        /// <summary>
        /// Configure the polygon storage implementations.
        /// </summary>
        /// <param name="services">The service collection.</param>
        void Configure(IServiceCollection services);
    }
}
