﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polygon.FakeJudgehost;
using System;
using System.Linq;

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
            services.AddOptions<DaemonOptions>();
            return services.AddSingleton<IHostedService, JudgeDaemon>(sp => new JudgeDaemon(sp, hostname, strategy));
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

        /// <summary>
        /// Creates a random password generator.
        /// </summary>
        /// <returns>The thread-unsafe random password generator.</returns>
        private static Func<string> CreatePasswordGenerator()
        {
            const string passwordSource = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789";
            var rng = new Random(unchecked((int)DateTimeOffset.Now.Ticks));
            return () =>
            {
                Span<char> pwd = stackalloc char[8];
                for (int i = 0; i < 8; i++) pwd[i] = passwordSource[rng.Next(passwordSource.Length)];
                return pwd.ToString();
            };
        }

        /// <summary>
        /// Generate random judgehost account and let the startup of fake judgehost finish the account creating.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddFakeJudgehostAccount(this IServiceCollection services)
        {
            var pwd = CreatePasswordGenerator().Invoke();
            
            services.ConfigureJudgeDaemon(options =>
            {
                options.Password = pwd;
                options.UserName = "judgehost-" + pwd[0..4];
            });

            services.AddSingleton<InitializeFakeJudgehostService>();

            return services;
        }

        /// <summary>
        /// Find the correct judgehost, or throwing an exception when not found.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="hostname">The judgehost hostname.</param>
        /// <returns>The judgehost.</returns>
        public static JudgeDaemon GetJudgehost(this IServiceProvider serviceProvider, string hostname)
        {
            return serviceProvider
                .GetServices<IHostedService>()
                .OfType<JudgeDaemon>()
                .Where(j => j.HostName == hostname)
                .Single();
        }
    }
}
