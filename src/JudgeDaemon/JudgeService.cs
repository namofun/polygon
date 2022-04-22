using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xylab.Management.Services;

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
        private readonly ISystemUtilities _system;

        private async Task registerJudgehost(string endpointId, Endpoint endpoint, string myhost)
        {
            // Only try to register every 30s.
            var now = DateTimeOffset.Now;
            if (now - endpoint.LastAttempt < TimeSpan.FromSeconds(30))
            {
                endpoint.Waiting = true;
                return;
            }

            endpoint.LastAttempt = now;

            _logger.LogInformation("Registering judgehost on endpoint {endpointID}: {url}", endpointId, endpoint.Url);
            endpoint.Client = setup_curl_handle(endpoint.UserName, endpoint.Password);

            // Create directory where to test submissions
            var workdirpath = $"{_options.JUDGEDIR}/{myhost}/endpoint-{endpointId}";
            try
            {
                Directory.CreateDirectory(workdirpath + "/testcase");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Could not create {workdirpath}", workdirpath);
            }

            _system.ChangeMode(Path.Combine(workdirpath, "testcase"), 0700);

            // Auto-register judgehost.
            // If there are any unfinished judgings in the queue in my name,
            // they will not be finished. Give them back.
            var unfinished = request('judgehosts', 'POST', 'hostname='.urlencode($myhost), false);
            if (unfinished == null)
            {
                _logger.LogWarning("Registering judgehost on endpoint {endpointID} failed.", endpointId);
            }
            else
            {
                foreach (var jud in unfinished)
                {
                    var workdir = judging_directory($workdirpath, $jud);
                    _system.ChangeMode(workdir, 0700);
                    _logger.LogWarning("Found unfinished judging j{id} in my name; given back", jud.judgingid);
                }
            }
        }

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
                        await registerJudgehost(id, endpoint, myhost);
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

                        int error_id = await domClient.FireInternalError(
                            $"low on disk space on {_myhost}",
                            judgehostlog,
                            DisableTarget.Judgehost(_myhost));

                        _logger.LogError("=> internal error {error_id}", error_id);
                    }
                }

                // Request open submissions to judge. Any errors will be treated as
                // non-fatal: we will just keep on retrying in this loop.
                NextJudging row = await domClient.FetchNextJudging(_myhost);

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
