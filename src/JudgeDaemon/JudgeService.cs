using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Xylab.Polygon.Judgement.Daemon
{
    public class JudgeService : BackgroundService
    {
        private readonly EndpointManager _endpoints;
        private readonly DaemonOptions _options;
        private readonly ILogger<JudgeService> _logger;

        public JudgeService(
            IOptions<DaemonOptions> options,
            ILogger<JudgeService> logger,
            EndpointManager endpoints)
        {
            _options = options.Value;
            _logger = logger;
            _endpoints = endpoints;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                Endpoint endpoint = await _endpoints.MoveNextAsync(stoppingToken);

                // Request open submissions to judge. Any errors will be treated as
                // non-fatal: we will just keep on retrying in this loop.
                NextJudging? row = await endpoint.FetchNextJudging(_options.HostName);

                // nothing returned -> no open submissions for us
                if (row == null)
                {
                    if (!endpoint.Waiting)
                    {
                        _logger.LogInformation("No submissions in queue (for endpoint {endpointID}), waiting...", endpoint.Name);
                        endpoint.Waiting = true;
                    }

                    continue;
                }

                // we have gotten a submission for judging
                endpoint.Waiting = false;

                _logger.LogInformation(
                    "Judging submission s{submitid} (endpoint {endpointID}) (t{teamid}/p{probid}/{langid}), id j{judgingid}...",
                    row.SubmissionId,
                    endpoint.Name,
                    row.TeamId,
                    row.ProblemId,
                    row.LanguageId,
                    row.JudgingId);

                await Judge(endpoint, row);

                // Check if we were interrupted while judging, if so, exit (to avoid sleeping)
                stoppingToken.ThrowIfCancellationRequested();

                // restart the judging loop
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received signal, exiting.");
            await base.StopAsync(cancellationToken);
            await _endpoints.DisposeAsync();
        }

        private Task Judge(Endpoint endpoint, NextJudging row)
        {
            return Task.CompletedTask;
        }
    }
}
