using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xylab.Management.Models;
using Xylab.Management.Services;

namespace Xylab.Polygon.Judgement.Daemon
{
    public interface ITaskRunner
    {
        Task<ProcessResult> Compile(
            string cpuset,
            string execrunpath,
            string workdir,
            IEnumerable<string> files);

        Task<ProcessResult> BuildAtCurrentDirectory();
    }

    public class DefaultTaskRunner : ITaskRunner
    {
        private readonly IProcessFactory _processFactory;
        private readonly DaemonOptions _options;

        public DefaultTaskRunner(IProcessFactory processFactory, DaemonOptions options)
        {
            _processFactory = processFactory;
            _options = options;
        }

        public Task<ProcessResult> BuildAtCurrentDirectory()
        {
            return _processFactory.StartAsync(
                "./build",
                cmdline: null,
                options: new ProcessStartupOptions
                {
                    UseMassiveOutput = true,
                });
        }

        public Task<ProcessResult> Compile(
            string cpuset,
            string execrunpath,
            string workdir,
            IEnumerable<string> files)
        {
            return _processFactory.StartAsync(
                Path.Combine(_options.LIBJUDGEDIR, "compile.sh"),
                $"{cpuset} '{execrunpath}' '{workdir}' " + string.Join(' ', files));
        }
    }
}
