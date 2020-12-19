using Microsoft.Extensions.DependencyInjection;
using Polygon.FakeJudgehost;
using System;

namespace Polygon
{
    /// <summary>
    /// Configure services for fake judgehost to run.
    /// </summary>
    public static class FakeJudgehostServiceCollectionExtensions
    {
        /// <summary>
        /// Configure the judgedaemon settings.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">The configure action.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection ConfigureJudgeDaemon(this IServiceCollection services, Action<DaemonOptions> configureOptions)
        {
            services.AddOptions<DaemonOptions>();
            return services.Configure(configureOptions);
        }

        /// <summary>
        /// Add a running judgedaemon.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="hostname">The hostname.</param>
        /// <param name="strategy">The execution logic.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddJudgehost(this IServiceCollection services, string hostname, IDaemonStrategy strategy)
        {
            return services.AddHostedService(sp => new JudgeDaemon(sp, hostname, strategy));
        }

        /// <summary>
        /// Add a running judgedaemon.
        /// </summary>
        /// <typeparam name="TStrategy">The execution logic type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="hostname">The hostname.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddJudgehost<TStrategy>(this IServiceCollection services, string hostname) where TStrategy : class, IDaemonStrategy, new()
        {
            return AddJudgehost(services, hostname, new TStrategy());
        }
    }
}
