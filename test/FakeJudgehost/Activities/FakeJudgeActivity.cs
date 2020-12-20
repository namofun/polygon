using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polygon.FakeJudgehost
{
    public class FakeJudgeActivity : IDaemonStrategy
    {
        public Task ExecuteAsync(JudgeDaemon service, CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
