using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xylab.Management.Models;
using Xylab.Management.Services;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Models;

namespace Xylab.Polygon.Judgement.Daemon
{
    public class JudgeService : BackgroundService
    {
        private readonly IEndpointManager _endpoints;
        private readonly DaemonOptions _options;
        private readonly ILogger<JudgeService> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly ITaskRunner _taskRunner;

        public JudgeService(
            IOptions<DaemonOptions> options,
            ILogger<JudgeService> logger,
            IFileSystem fileSystem,
            ITaskRunner taskRunner,
            IEndpointManager endpoints)
        {
            _options = options.Value;
            _logger = logger;
            _endpoints = endpoints;
            _fileSystem = fileSystem;
            _taskRunner = taskRunner;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                IEndpoint endpoint = await _endpoints.MoveNextAsync(stoppingToken);

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

        private async Task<Dictionary<string, string>?> read_metadata(string filename)
        {
            if (!_fileSystem.IsReadable(filename)) return null;

            // Don't quite treat it as YAML, but simply key/value pairs.
            string[] contents = await _fileSystem.ReadFileAsLines(filename);
            Dictionary<string, string> res = new();
            foreach (var line in contents)
            {
                int strpos = line.IndexOf(':');
                if (strpos != -1)
                {
                    res.Add(line[0..strpos], line[(strpos + 1)..].Trim());
                }
            }

            return res;
        }


        private async Task<(string? runpath, string? error)> FetchExecutable(
            IEndpoint endpoint,
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

            if (!_fileSystem.FileExists(execpath)
                || !_fileSystem.FileExists(execmd5path)
                || !_fileSystem.FileExists(execdeploypath)
                || md5sum != await _fileSystem.ReadFileContent(execmd5path))
            {
                _logger.LogInformation("Fetching new executable '{execid}'", execid);
                await _fileSystem.DeleteDirectoryRecursive(execpath);
                if (!_fileSystem.CreateDirectory(execpath, true))
                {
                    throw new ApplicationException($"Could not create directory '{execpath}'");
                }

                byte[] content = await endpoint.GetExecutable(execid);
                if (!await _fileSystem.WriteFile(execzippath, content))
                {
                    throw new ApplicationException($"Could not create executable zip file in {execpath}");
                }

                if (!string.Equals(content.ToMD5().ToHexDigest(), md5sum))
                {
                    throw new ApplicationException("Zip file corrupted during download.");
                }

                if (!await _fileSystem.WriteFile(execmd5path, md5sum))
                {
                    throw new ApplicationException("Could not write md5sum to file.");
                }

                _logger.LogDebug("Unzipping");
                /*        system("unzip -Z $execzippath | grep -q ^l", $retval);
        if ($retval===0) {
            error("Zipfile $execzippath contains symlinks");
        }
        system("unzip -j -q -d $execpath $execzippath", $retval);
        if ($retval!=0) {
            error("Could not unzip zipfile in $execpath");
        }*/

                bool do_compile = true;
                if (!_fileSystem.FileExists(execbuildpath))
                {
                    if (_fileSystem.FileExists(execrunpath))
                    {
                        _logger.LogDebug("'run' exists without 'build', we are done");
                        do_compile = false;
                    }
                    else
                    {
                        Dictionary<string, string[]> langexts = new()
                        {
                            ["c"] = new[] { "c" },
                            ["cpp"] = new[] { "cpp", "C", "cc" },
                            ["java"] = new[] { "java" },
                            ["py"] = new[] { "py", "py2", "py3" },
                        };

                        StringBuilder buildScript = new();
                        buildScript.Append("#!/bin/sh\n\n");
                        string? execlang = null;
                        string source = "";

                        foreach (var (lang, langext) in langexts)
                        {
                            /*if (($handle = opendir($execpath)) === false) {
                        error("Could not open $execpath");
                    }
                    while (($file = readdir($handle)) !== false) {
                        $ext = pathinfo($file, PATHINFO_EXTENSION);
                        if (in_array($ext, $langext)) {
                            $execlang = $lang;
                            $source = $file;
                            break;
                        }
                    }
                    closedir($handle);
                    if ($execlang !== false) {
                        break;
                    }
                             */
                        }

                        if (execlang == null)
                        {
                            return (null, "executable must either provide an executable file named 'build' or a C/C++/Java or Python file.");
                        }

                        switch (execlang)
                        {
                            case "c":
                                buildScript.Append($"gcc -Wall -O2 -std=gnu11 '{source}' -o {execrunpath} -lm\n");
                                break;

                            case "cpp":
                                buildScript.Append($"g++ -Wall -O2 -std=gnu++17 '{source}' -o {execrunpath}\n");
                                break;

                            case "java":
                                //source = basename(source, ".java");
                                buildScript.Append($"javac -cp {execpath} -d {execpath} '{source}'.java\n");
                                buildScript.Append("echo '#!/bin/sh' > run\n");
                                // no main class detection here
                                buildScript.Append($"echo 'java -cp {execpath} '{source} >> run\n");
                                break;

                            case "py":
                                buildScript.Append("echo '#!/bin/sh' > run\n");
                                // TODO: Check if it's 'python' or 'python3'
                                buildScript.Append($"echo 'python '{source} >> run\n");
                                break;
                        }

                        if (combined_run_compare)
                        {
                            string run_runjury = null;
                            buildScript.Append(run_runjury);
                        }

                        try
                        {
                            await _fileSystem.WriteFile(execbuildpath, buildScript.ToString());
                        }
                        catch (IOException ex)
                        {
                            throw new ApplicationException($"Could not write file 'build' in {execpath}", ex);
                        }

                        _fileSystem.ChangeMode(execbuildpath, 0755);
                    }
                }
                else if (!_fileSystem.IsExecutable(execbuildpath))
                {
                    return (null, "Invalid executable, file 'build' exists but is not executable.");
                }

                if (do_compile)
                {
                    _logger.LogDebug("Compiling");
                    string olddir = Environment.CurrentDirectory;
                    Environment.CurrentDirectory = execpath;
                    _fileSystem.ChangeMode("./build", 0750);

                    var result = await _taskRunner.BuildAtCurrentDirectory();
                    if (result.ExitCode != 0)
                    {
                        return (null, "Could not run ./build in $execpath.");
                    }

                    _fileSystem.ChangeMode(execrunpath, 0755);
                    Environment.CurrentDirectory = olddir;
                }

                if (!_fileSystem.FileExists(execrunpath) || !_fileSystem.IsExecutable(execrunpath))
                {
                    return (null, "Invalid build file, must produce an executable file 'run'.");
                }
            }

            // Create file to mark executable successfully deployed.
            await _fileSystem.WriteFile(execdeploypath, Array.Empty<byte>());

            return (execrunpath, null);
        }

        private async Task Judge(IEndpoint endpoint, NextJudging row)
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

            int outputStorageLimit = await endpoint.GetConfiguration(c => c.OutputStorageLimit);
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

            try
            {
                Environment.CurrentDirectory = workdir;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Could not chdir to '{workdir}'", ex);
            }

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

                if (!await _fileSystem.WriteFile(srcfile, Convert.FromBase64String(source.SourceCode)))
                {
                    throw new ApplicationException($"Could not create {srcfile}");
                }
            }

            if (files.Count == 0 && hasFiltered)
            {
                await endpoint.UpdateJudging(
                    _options.HostName,
                    row.JudgingId,
                    false,
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

            var (execrunpath, error) = await FetchExecutable(endpoint, workdirpath, row.Compile, row.CompareMd5sum);
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

            // Compile the program.
            ProcessResult compileResult = await _taskRunner.Compile(cpusetOption, execrunpath, workdir, files);

            int retval = compileResult.ExitCode;
            string compile_output = "";
            if (_fileSystem.IsReadable(Path.Combine(workdir, "compile.out")))
            {
                compile_output = await _fileSystem.ReadFileWithLimit(Path.Combine(workdir, "/compile.out"), 50000);
            }
            if (string.IsNullOrEmpty(compile_output) && _fileSystem.IsReadable(Path.Combine(workdir, "compile.tmp")))
            {
                compile_output = await _fileSystem.ReadFileWithLimit(Path.Combine(workdir, "/compile.tmp"), 50000);
            }

            // Try to read metadata from file
            Dictionary<string, string> metadata = await read_metadata(Path.Combine(workdir, "/compile.meta"));
            if (metadata.ContainsKey("internal-error"))
            {
                //alert('error');
                string internalError = metadata["internal-error"];
                string description;
                compile_output += "\n--------------------------------------------------------------------------------\n\n"
                                + "Internal errors reported:\n"
                                + internalError;

                if (internalError.Contains("compile script: "))
                {
                    internalError = internalError.Replace("compile script: ", string.Empty);
                    description = $"The compile script returned an error: {internalError}";
                    await endpoint.FireInternalError(description, "kusto", DisableTarget.Language(row.LanguageId), row, compile_output);
                }
                else
                {
                    description = $"Running compile.sh caused an error/crash: {internalError}";
                    await endpoint.FireInternalError(description, "kusto", DisableTarget.Judgehost(_options.HostName), row, compile_output);
                }

                _logger.LogError(description);
                // revoke readablity for domjudge-run user to this workdir
                _fileSystem.ChangeMode(workdir, 0700);
                return;
            }

            // What does the exitcode mean?
            if (!Enum.GetValues<ExitCodes>().Any(e => (int)e == retval))
            {
                //alert('error');
                _logger.LogError("Unknown exitcode from compile.sh for s{submitid}: {retval}", row.SubmissionId, retval);

                await endpoint.FireInternalError(
                    $"compile script '{row.Compile}' returned exit code {retval}",
                    "kusto query",
                    DisableTarget.Language(row.LanguageId),
                    row,
                    compile_output);
                
                // revoke readablity for domjudge-run user to this workdir
                _fileSystem.ChangeMode(workdir, 0700);
                return;
            }
            bool compile_success = (ExitCodes)retval == ExitCodes.Correct;

            // pop the compilation result back into the judging table
            await endpoint.UpdateJudging(
                _options.HostName,
                row.JudgingId,
                compile_success,
                await _fileSystem.ReadFileWithLimit(Path.Combine(workdir, "compile.out"), outputStorageLimit),
                metadata.GetValueOrDefault("entry_point"));

            // compile error: our job here is done
            if (!compile_success)
            {
                // revoke readablity for domjudge-run user to this workdir
                _fileSystem.ChangeMode(workdir, 0700);
                _logger.LogInformation("Judging s{submitid}/j{judgingid}: compile error", row.SubmissionId, row.JudgingId);
                return;
            }


            throw new NotImplementedException();
        }
    }
}
