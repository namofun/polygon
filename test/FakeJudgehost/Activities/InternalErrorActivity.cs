using System.Threading;
using System.Threading.Tasks;

namespace Xylab.Polygon.Judgement.Daemon.Fake
{
    public class InternalErrorActivity : IDaemonStrategy
    {
        public SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(0, 1);

        public async Task ExecuteAsync(JudgeDaemon service, CancellationToken stoppingToken)
        {
            await service.Disable(
                "judgehost", "hostname", service.HostName,
                $"low on disk space on {service.HostName}",
                extra_log: "[Oct 24 15:06:43.989] judgedaemon[506]: Low on disk space: 1.00GB free, clean up or change 'diskspace error' value in config before resolving this error.");

            Semaphore.Release();
        }
    }
}
