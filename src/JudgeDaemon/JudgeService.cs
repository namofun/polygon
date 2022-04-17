﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xylab.Polygon.Judgement.Daemon
{
    public class JudgeService : BackgroundService
    {
        private readonly DaemonOptions _options;
        private readonly TimeSpan waittime = TimeSpan.FromSeconds(5);
        private readonly IReadOnlyDictionary<string, Endpoint> _endpoints;
        private readonly IReadOnlyList<string> _endpointIDs;
        private readonly string _myhost;
        private readonly ILogger<JudgeService> _logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int currentEndpoint = 0;
            string endpointID, workdirpath;
            while (true)
            {
                // If all endpoints are waiting, sleep for a bit
                bool dosleep = true;
                foreach (var (id, endpoint) in _endpoints)
                {
                    if (endpoint.Errorred)
                    {
                        endpointID = id;
                        registerJudgehost(@myhost);
                    }

                    if (!endpoint.Waiting)
                    {
                        dosleep = false;
                        break;
                    }
                }

                // Sleep only if everything is "waiting" and only if we're looking at the first endpoint again
                if (dosleep && currentEndpoint == 0)
                {
                    await Task.Delay(@waittime, stoppingToken);
                }

                // Increment our currentEndpoint pointer
                currentEndpoint = (currentEndpoint + 1) % _endpoints.Count;
                endpointID = _endpointIDs[currentEndpoint];
                workdirpath = Path.Combine(_options.JUDGEDIR, _myhost, "endpoint-" + endpointID);

                // Check whether we have received an exit signal
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Received signal, exiting.");
                    close_curl_handles();
                    return;
                }

                if (_endpoints[endpointID].Errorred)
                {
                    continue;
                }

                DomClient domClient = null;

                if (!_endpoints[endpointID].Waiting)
                {
                    // Check for available disk space
                    long free_space = disk_free_space(_options.JUDGEDIR);
                    long allowed_free_space = djconfig_get_value("diskspace_error"); // in kB
                    if (@free_space < 1024 * @allowed_free_space)
                    {
                        string free_abs = $"{free_space / (double)(1024 * 1024 * 1024):F2}GB";
                        _logger.LogError("Low on disk space: {free_abs} free, clean up or change 'diskspace error' value in config before resolving this error.", free_abs);

                        int error_id = await domClient.FireInternalErrorAsync(
                            $"low on disk space on {_myhost}",
                            judgehostlog,
                            DisableTarget.Judgehost(_myhost));

                        _logger.LogError("=> internal error {error_id}", error_id);
                    }
                }

                // Request open submissions to judge. Any errors will be treated as
                // non-fatal: we will just keep on retrying in this loop.
                NextJudging row = await domClient.FetchNextJudgingAsync(_myhost);

                // nothing returned -> no open submissions for us
                if (row == null)
                {
                    if (!_endpoints[endpointID].Waiting)
                    {
                        _logger.LogInformation("No submissions in queue (for endpoint {endpointID}), waiting...", endpointID);
                        _endpoints[endpointID].Waiting = true;
                    }

                    continue;
                }

                // we have gotten a submission for judging
                _endpoints[endpointID].Waiting = false;

                _logger.LogInformation(
                    "Judging submission s{submitid} (endpoint {endpointID}) (t{teamid}/p{probid}/{langid}), id j{judgingid}...",
                    row.SubmissionId,
                    endpointID,
                    row.TeamId,
                    row.ProblemId,
                    row.LanguageId,
                    row.JudgingId);

                judge(row);

                // Check if we were interrupted while judging, if so, exit (to avoid sleeping)
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Received signal, exiting.");
                    close_curl_handles();
                    return;
                }

                // restart the judging loop
            }
        }

        private void close_curl_handles() { }

        private void judge(NextJudging row) { }

        private long disk_free_space(string dir) { }

        private long djconfig_get_value(string name) { }
    }
}
