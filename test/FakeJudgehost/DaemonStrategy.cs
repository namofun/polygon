using System.Threading;
using System.Threading.Tasks;

namespace Polygon.FakeJudgehost
{
    /// <summary>
    /// Abstraction for many daemon events like internal error, interrupt when judging, or so on.
    /// </summary>
    public interface IDaemonStrategy
    {
        /// <summary>
        /// Execute the daemon action.
        /// </summary>
        /// <param name="service">The judge daemon.</param>
        /// <param name="stoppingToken">The token to stop action.</param>
        /// <returns>The task for executing core logics.</returns>
        Task ExecuteAsync(JudgeDaemon service, CancellationToken stoppingToken);
    }
}
