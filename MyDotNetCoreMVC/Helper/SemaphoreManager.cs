using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace MyDotNetCoreMVC.Helpers
{
    public class SemaphoreManager
    {
        private class SemaphoreWithCount
        {
            public SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

            public int Count = 0;
        }

        private readonly ConcurrentDictionary<int, SemaphoreWithCount> SemaphoreDict = new ConcurrentDictionary<int, SemaphoreWithCount>();

        public async Task<IDisposable> AcquireAsync(int id)
        {
            var semaphoreWithCount = SemaphoreDict.GetOrAdd(id, _ => new SemaphoreWithCount());
            Interlocked.Increment(ref semaphoreWithCount.Count);

            await semaphoreWithCount.Semaphore.WaitAsync();

            return new ReleaseWrapper(() => Release(id));
        }

        public bool IsLockIdPresent(int id)
        {
            return SemaphoreDict.ContainsKey(id);
        }

        private void Release(int id)
        {
            if (SemaphoreDict.TryGetValue(id, out var semaphoreWithCount))
            {
                if (Interlocked.Decrement(ref semaphoreWithCount.Count) == 0)
                {
                    SemaphoreDict.TryRemove(id, out _);
                    semaphoreWithCount.Semaphore.Release();
                }
            }
        }

        private class ReleaseWrapper : IDisposable
        {
            private readonly Action releaseAction;

            public ReleaseWrapper(Action releaseAction)
            {
                this.releaseAction = releaseAction;
            }

            public void Dispose()
            {
                releaseAction();
            }
        }
    }

}
