using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    /// <summary>
    /// The locker for asynchronous functions using <see cref="SemaphoreSlim"/> with concurrent degree.
    /// </summary>
    internal class ConcurrentAsyncLock
    {
        private readonly AsyncLock[] _locks;

        /// <summary>
        /// Instantiate a <see cref="ConcurrentAsyncLock"/>.
        /// </summary>
        /// <param name="degree">The concurrent degree.</param>
        public ConcurrentAsyncLock(int degree = 20)
        {
            _locks = new AsyncLock[degree];
            for (int i = 0; i < degree; i++)
                _locks[i] = new AsyncLock();
        }

        /// <summary>
        /// Wait for the critical section.
        /// </summary>
        /// <param name="hashCode">The hash code.</param>
        /// <returns>The disposer to release from critical section.</returns>
        public Task<IDisposable> LockAsync(int hashCode)
        {
            return _locks[hashCode % _locks.Length].LockAsync();
        }
    }
}
