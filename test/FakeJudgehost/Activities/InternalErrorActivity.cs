using System.Threading;
using System.Threading.Tasks;

namespace Polygon.FakeJudgehost
{
    public class InternalErrorActivity : IDaemonStrategy
    {
        public Task ExecuteAsync(JudgeDaemon service, CancellationToken stoppingToken)
        {
            return service.Disable(
                "judgehost", "hostname", service.HostName,
                $"low on disk space on {service.HostName}",
                extra_log: "[Oct 24 15:06:43.989] judgedaemon[506]: Low on disk space: 1.00GB free, clean up or change 'diskspace error' value in config before resolving this error.");
        }
    }
}
