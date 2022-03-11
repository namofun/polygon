using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using Xylab.Polygon.Judgement.Daemon.Fake;

namespace Xylab.Polygon
{
    /// <summary>
    /// The builder for fake judgehost.
    /// </summary>
    public class FakeJudgehostBuilder
    {
        /// <summary>
        /// The service collection
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Initialize the fake judgehost builder.
        /// </summary>
        /// <param name="services">The service collection.</param>
        internal FakeJudgehostBuilder(IServiceCollection services)
        {
            Services = services;
            services.AddOptions<DaemonOptions>();
        }

        /// <summary>
        /// Generate random judgehost account and let the startup of fake judgehost finish the account creating.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>The builder to chain calls.</returns>
        public FakeJudgehostBuilder AddRealAccount(string username, string password)
        {
            return ConfigureJudgeDaemon(options =>
            {
                options.Password = password;
                options.UserName = username;
            });
        }

        /// <summary>
        /// Configure the judgedaemon settings.
        /// </summary>
        /// <param name="configureOptions">The configure action.</param>
        /// <returns>The service collection.</returns>
        public FakeJudgehostBuilder ConfigureJudgeDaemon(Action<DaemonOptions> configureOptions)
        {
            Services.Configure(configureOptions);
            return this;
        }

        /// <summary>
        /// Add a running judgedaemon.
        /// </summary>
        /// <param name="hostname">The hostname.</param>
        /// <param name="strategy">The execution logic.</param>
        /// <returns>The service collection.</returns>
        public FakeJudgehostBuilder AddJudgehost(string hostname, IDaemonStrategy strategy)
        {
            Services.AddSingleton<IHostedService, JudgeDaemon>(sp => new JudgeDaemon(sp, hostname, strategy));
            return this;
        }

        /// <summary>
        /// Add a running judgedaemon.
        /// </summary>
        /// <typeparam name="TStrategy">The execution logic type.</typeparam>
        /// <param name="hostname">The hostname.</param>
        /// <returns>The service collection.</returns>
        public FakeJudgehostBuilder AddJudgehost<TStrategy>(string hostname) where TStrategy : class, IDaemonStrategy, new()
        {
            return AddJudgehost(hostname, new TStrategy());
        }

        /// <summary>
        /// Add the <see cref="HttpClient"/> factory.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <returns>The service collection.</returns>
        public FakeJudgehostBuilder AddHttpClientFactory(Func<IServiceProvider, HttpClient> factory)
        {
            return ConfigureJudgeDaemon(options =>
            {
                options.HttpClientFactory = factory;
            });
        }
    }
}
