using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polygon.Judgement;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Polygon.FakeJudgehost
{
    /// <summary>
    /// The judgedaemon to host judgehost actions.
    /// </summary>
    public class JudgeDaemon : BackgroundService
    {
        private readonly Lazy<HttpClient> _httpClient;
        private bool _started;

        /// <summary>
        /// The judgehost execution logic
        /// </summary>
        public IDaemonStrategy Strategy { get; }

        /// <summary>
        /// The daemon options
        /// </summary>
        public IOptions<DaemonOptions> Options { get; }

        /// <summary>
        /// The service provider (Singleton)
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// The servicing HTTP client
        /// </summary>
        public HttpClient HttpClient => _httpClient.Value;

        /// <summary>
        /// The judgehost host name
        /// </summary>
        public string HostName { get; }

        /// <summary>
        /// The logger to log things
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Whether error happened
        /// </summary>
        public bool Error { get; internal set; }

        /// <summary>
        /// Creates a <see cref="JudgeDaemon"/> using the hostname and execution strategy.
        /// </summary>
        /// <param name="services">The service provider.</param>
        /// <param name="hostname">The hostname.</param>
        /// <param name="strategy">The execution strategy.</param>
        public JudgeDaemon(IServiceProvider services, string hostname, IDaemonStrategy strategy)
        {
            Services = services;
            Options = services.GetRequiredService<IOptions<DaemonOptions>>();
            HostName = hostname;
            _httpClient = new Lazy<HttpClient>(() => Options.Value.CreateConfiguredClient(Services));
            Strategy = strategy;
            Logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Polygon.FakeJudgehost." + hostname);
        }

        /// <inheritdoc cref="BackgroundService.StartAsync(CancellationToken)" />
        public Task ManualStartAsync(CancellationToken cancellationToken = default)
        {
            if (_started) return Task.CompletedTask;
            return base.StartAsync(cancellationToken);
        }

        /// <inheritdoc />
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var init = Services.GetService<IInitializeFakeJudgehostService>();
            if (init != null) await init.EnsureAsync();

            var env = Services.GetRequiredService<IHostEnvironment>();
            if (!env.IsEnvironment("Testing"))
            {
                await base.StartAsync(cancellationToken);
                _started = true;
            }
            else
            {
                Logger.LogWarning(
                    "This judgehost has not been started.\r\n" +
                    "Please await IServiceProvider.GetJudgehost(\"{hostname}\").ManualStartAsync();",
                    HostName);
            }
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                try
                {
                    Logger.LogInformation("Registering judgehost on endpoint...");

                    var result = await this.Register();
                    foreach (var item in result)
                    {
                        Logger.LogWarning("Found unfinished judging j{judgingId} in my name; given back", item.JudgingId);
                    }

                    break;
                }
                catch
                {
                    Logger.LogError("Registering judgehost on endpoint failed.");
                    await Task.Delay(1000, stoppingToken);
                }
            }

            await Strategy.ExecuteAsync(this, stoppingToken);
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            HttpClient?.Dispose();
            base.Dispose();
        }
    }
}
