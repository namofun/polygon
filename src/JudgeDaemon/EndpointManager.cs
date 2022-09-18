using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Xylab.Polygon.Judgement.Daemon
{
    public sealed class EndpointManager : IAsyncDisposable
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DaemonOptions _options;
        private readonly ILogger<EndpointManager> _logger;
        private readonly IReadOnlyList<Endpoint> _endpoints;
        private readonly IFileSystem _fileSystem;
        private int _currentEndpoint;

        public EndpointManager(
            IOptions<DaemonOptions> options,
            ILogger<EndpointManager> logger,
            IFileSystem fileSystem,
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
            _endpoints = options.Value.Endpoints.ToList();
            _fileSystem = fileSystem;
            _currentEndpoint = -1;
        }

        private async Task RegisterJudgehost(Endpoint endpoint)
        {
            // Only try to register every 30s.
            var now = DateTimeOffset.Now;
            if (now - endpoint.LastAttempt < TimeSpan.FromSeconds(30))
            {
                endpoint.Waiting = true;
                return;
            }

            endpoint.LastAttempt = now;

            _logger.LogInformation("Registering judgehost on endpoint {endpointID}: {url}", endpoint.Name, endpoint.Url);
            try
            {
                endpoint.Initialize(_httpClientFactory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception happened during creating http client.");
                throw;
            }

            // Create directory where to test submissions
            string workdirpath = Path.Combine(_options.JUDGEDIR, _options.HostName, $"endpoint-{endpoint.Name}");
            try
            {
                _fileSystem.CreateDirectory(Path.Combine(workdirpath, "testcase"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Could not create {workdirpath}", workdirpath);
            }

            _fileSystem.ChangeMode(Path.Combine(workdirpath, "testcase"), 0700);

            // Auto-register judgehost.
            // If there are any unfinished judgings in the queue in my name,
            // they will not be finished. Give them back.
            var unfinished = await endpoint.RegisterJudgehost(_options.HostName);
            if (unfinished == null)
            {
                _logger.LogWarning("Registering judgehost on endpoint {endpointID} failed.", endpoint.Name);
            }
            else
            {
                foreach (UnfinishedJudging jud in unfinished)
                {
                    var workdir = jud.GetJudgehostPath(workdirpath);
                    _fileSystem.ChangeMode(workdir, 0700);
                    _logger.LogWarning("Found unfinished judging j{id} in my name; given back", jud.JudgingId);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            foreach (Endpoint endpoint in _endpoints)
            {
                await endpoint.DisposeAsync();
            }
        }

        public async ValueTask<Endpoint> MoveNextAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                bool dosleep = true;
                foreach (Endpoint current in _endpoints)
                {
                    if (current.Errorred)
                    {
                        await RegisterJudgehost(current);
                    }

                    if (!current.Waiting)
                    {
                        dosleep = false;
                        break;
                    }
                }

                // Sleep only if everything is "waiting" and only if we're looking at the first endpoint again
                if (dosleep && _currentEndpoint == 0)
                {
                    await Task.Delay(_options.WaitTime, cancellationToken);
                }

                // Increment our currentEndpoint pointer
                _currentEndpoint = (_currentEndpoint + 1) % _endpoints.Count;
                Endpoint endpoint = _endpoints[_currentEndpoint];

                // Check whether we have received an exit signal
                cancellationToken.ThrowIfCancellationRequested();

                if (endpoint.Errorred)
                {
                    continue;
                }

                if (!endpoint.Waiting)
                {
                    // Check for available disk space
                    long free_space = _fileSystem.GetFreeSpace(_options.JUDGEDIR);
                    long allowed_free_space = await endpoint.GetConfiguration(c => c.DiskSpaceError); // in kB
                    if (free_space < 1024 * allowed_free_space)
                    {
                        string free_abs = $"{free_space / (double)(1024 * 1024 * 1024):F2}GB";
                        _logger.LogError("Low on disk space: {free_abs} free, clean up or change 'diskspace error' value in config before resolving this error.", free_abs);

                        int error_id = await endpoint.FireInternalError(
                            $"low on disk space on {_options.HostName}",
                            "kusto query",
                            DisableTarget.Judgehost(_options.HostName));

                        _logger.LogError("=> internal error {error_id}", error_id);
                    }

                    // After such internal error created, judgehost will be disabled in domserver and no judging will be distrubuted, so waiting will be always true.
                }

                return endpoint;
            }
        }
    }
}
