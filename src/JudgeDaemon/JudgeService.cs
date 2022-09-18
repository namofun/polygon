using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xylab.Polygon.Models;

namespace Xylab.Polygon.Judgement.Daemon
{
    public class JudgeService : BackgroundService
    {
        private readonly EndpointManager _endpoints;
        private readonly DaemonOptions _options;
        private readonly ILogger<JudgeService> _logger;
        private readonly IFileSystem _fileSystem;

        public JudgeService(
            IOptions<DaemonOptions> options,
            ILogger<JudgeService> logger,
            IFileSystem fileSystem,
            EndpointManager endpoints)
        {
            _options = options.Value;
            _logger = logger;
            _endpoints = endpoints;
            _fileSystem = fileSystem;
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

        private async Task<(string? runpath, string? error)> FetchExecutable(
            string workdirpath,
            string execid,
            string md5sum,
            bool combined_run_compare = false)
        {
            string execpath = Path.Combine(workdirpath, "executable", execid);
            string execmd5path = Path.Combine(execpath, "md5sum");
            string execdeploypath = Path.Combine(execpath, ".deployed");
            string execbuildpath = Path.Combine(execpath, "build");
            string execrunpath = Path.Combine(execpath, "run");
            string execzippath = Path.Combine(execpath, "executable.zip");
            if (string.IsNullOrEmpty(md5sum))
            {
                return (null, $"unknown executable '{execid}' specified");
            }

            throw new NotImplementedException();
        }

        private async Task Judge(Endpoint endpoint, NextJudging row)
        {
            string workdirpath = Path.Combine(_options.JUDGEDIR, _options.HostName, $"endpoint-{endpoint.Name}");

            // refresh config at start of judge run
            await endpoint.RefreshConfiguration();

            // Set configuration variables for called programs
            Environment.SetEnvironmentVariable("CREATE_WRITABLE_TEMP_DIR", _options.CREATE_WRITABLE_TEMP_DIR ? "1" : string.Empty);
            Environment.SetEnvironmentVariable("SCRIPTTIMELIMIT", await endpoint.GetConfiguration(c => c.ScriptTimeLimit.ToString()));
            Environment.SetEnvironmentVariable("SCRIPTMEMLIMIT", await endpoint.GetConfiguration(c => c.ScriptMemoryLimit.ToString()));
            Environment.SetEnvironmentVariable("SCRIPTFILELIMIT", await endpoint.GetConfiguration(c => c.ScriptFileSizeLimit.ToString()));
            Environment.SetEnvironmentVariable("MEMLIMIT", row.MemoryLimit.ToString());
            Environment.SetEnvironmentVariable("FILELIMIT", row.OutputLimit.ToString());
            Environment.SetEnvironmentVariable("PROCLIMIT", await endpoint.GetConfiguration(c => c.ProcessLimit.ToString()));
            Environment.SetEnvironmentVariable("ENTRY_POINT", row.EntryPoint);

            long outputStorageLimit = await endpoint.GetConfiguration(c => c.OutputStorageLimit);
            string cpusetOption = _options.daemonid.HasValue ? $"-n {_options.daemonid}" : string.Empty;

            // create workdir for judging
            string workdir = Path.Combine(workdirpath, row.ContestId.ToString(), row.SubmissionId.ToString(), row.JudgingId.ToString());
            _logger.LogInformation("Working directory: {workdir}", workdir);

            // If a database gets reset without removing the judging
            // directories, we might hit an old directory: rename it.
            if (_fileSystem.FileExists(workdir))
            {
                string oldworkdir = $"{workdir}-old-{Environment.ProcessId}-{DateTimeOffset.UtcNow:yyyy-MM-dd'_'HH':'mm}";
                if (!_fileSystem.Rename(workdir, oldworkdir))
                {
                    throw new ApplicationException("Could not rename stale working directory to '$oldworkdir'");
                }

                _fileSystem.ChangeMode(oldworkdir, 0700);
                _logger.LogWarning("Found stale working directory; renamed to '{oldworkdir}'", oldworkdir);
            }

            if (!_fileSystem.CreateDirectory(Path.Combine(workdir, "compile")))
            {
                throw new ApplicationException($"Could not create '{workdir}/compile'");
            }

            // Make sure the workdir is accessible for the domjudge-run user.
            // Will be revoked again after this run finished.
            _fileSystem.ChangeMode(workdir, 0755);

            Environment.CurrentDirectory = workdir;
            // if (!chdir(workdir)) error("Could not chdir to '$workdir'");

            // Get the source code from the DB and store in local file(s)
            var sources = await endpoint.GetSourceCode(row.ContestId, row.SubmissionId);
            List<string> files = new();
            bool hasFiltered = false;
            foreach (SubmissionFile source in sources)
            {
                string srcfile = Path.Combine(workdir, "compile", source.FileName);
                string file = source.FileName;

                if (row.FilterCompilerFiles ?? false)
                {
                    bool picked = false;
                    foreach (string extension in row.LanguageExtensions!)
                    {
                        int extensionLength = extension.Length;
                        if (source.FileName.EndsWith(extension))
                        {
                            files.Add(source.FileName);
                            picked = true;
                            break;
                        }
                    }

                    if (!picked)
                    {
                        hasFiltered = true;
                    }
                }
                else
                {
                    files.Add(source.FileName);
                }

                if (!await _fileSystem.WriteFileAsync(srcfile, Convert.FromBase64String(source.SourceCode)))
                {
                    throw new ApplicationException($"Could not create {srcfile}");
                }
            }

            if (files.Count == 0 && hasFiltered)
            {
                await endpoint.UpdateJudging(
                    _options.HostName,
                    row.JudgingId,
                    0,
                    "No files with allowed extensions found to pass to compiler. Allowed extensions: " + string.Join(", ", row.LanguageExtensions!));

                // revoke readablity for domjudge-run user to this workdir
                _fileSystem.ChangeMode(workdir, 0700);
                _logger.LogInformation("Judging s{submitid}/j{judgingid}: compile error", row.SubmissionId, row.JudgingId);
                return;
            }

            if (files.Count == 0)
            {
                throw new ApplicationException("No submission files could be downloaded.");
            }

            if (string.IsNullOrEmpty(row.Compile))
            {
                throw new ApplicationException($"No compile script specified for language {row.LanguageId}.");
            }

            var (execrunpath, error) = await FetchExecutable(workdirpath, row.Compile, row.CompareMd5sum);
            if (error != null)
            {
                _logger.LogError("fetching executable failed for compile script '{compile_script}': {error}", row.Compile, error);
                string description = row.Compile + ": fetch, compile, or deploy of compile script failed.";

                await endpoint.FireInternalError(
                    row.Compile + ": fetch, compile, or deploy of compile script failed.",
                    "kusto query",
                    DisableTarget.Language(row.LanguageId));
                //disable('language', 'langid', $row['langid'], $description, $row['judgingid'], (string)$row['cid']);
                return;
            }
            throw new NotImplementedException();
        }
    }
}
