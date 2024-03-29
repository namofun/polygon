﻿using System.Threading;
using System.Threading.Tasks;

namespace Xylab.Polygon.Judgement.Daemon.Fake
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

        /// <summary>
        /// The Semaphore to notify activity progress
        /// </summary>
        SemaphoreSlim Semaphore { get; }
    }
}
