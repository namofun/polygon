using System;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    public interface IConcurrencyGuard : IDisposable
    {
        Task<IDisposable> EnterCriticalSectionAsync();
    }

    public sealed class NotConcurrencyGuard : IConcurrencyGuard
    {
        public void Dispose()
        {
        }

        public Task<IDisposable> EnterCriticalSectionAsync()
        {
            return Task.FromResult<IDisposable>(this);
        }
    }

    public sealed class AsyncLockConcurrencyGuard : IConcurrencyGuard
    {
        public AsyncLock Lock { get; } = new();

        public void Dispose()
        {
            Lock.Dispose();
        }

        public Task<IDisposable> EnterCriticalSectionAsync()
        {
            return Lock.LockAsync();
        }
    }
}
