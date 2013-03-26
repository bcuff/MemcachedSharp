using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemcachedSharp
{
    internal class AsyncSemaphore
    {
        readonly Queue<Waiter> _waiters = new Queue<Waiter>();
        int _count;

        public AsyncSemaphore(int count)
        {
            _count = count;
        }

        internal int Count
        {
            get { return _count; }
        }

        public Task Wait()
        {
            return Wait(new Waiter
            {
                Source = new TaskCompletionSource<IDisposable>(),
                Result = null,
            });
        }

        public Task<IDisposable> WaitAndSignal()
        {
            return Wait(new Waiter
            {
                Source = new TaskCompletionSource<IDisposable>(),
                Result = new SignalDisposable(this),
            });
        }

        Task<IDisposable> Wait(Waiter waiter)
        {
            bool success;
            lock (_waiters)
            {
                if (_count > 0)
                {
                    --_count;
                    success = true;
                }
                else
                {
                    success = false;
                    _waiters.Enqueue(waiter);
                }
            }

            if (success)
            {
                waiter.Source.SetResult(waiter.Result);
            }

            return waiter.Source.Task;
        }

        public void Signal(int count = 1)
        {
            for (; count > 0; --count)
            {
                Waiter? waiter = null;
                lock (_waiters)
                {
                    if (_waiters.Count > 0)
                    {
                        waiter = _waiters.Dequeue();
                    }
                    else
                    {
                        ++_count;
                    }
                }
                if (waiter.HasValue)
                {
                    waiter.Value.Source.SetResult(waiter.Value.Result);
                }
            }
        }

        private struct Waiter
        {
            public TaskCompletionSource<IDisposable> Source;
            public IDisposable Result;
        }

        private class SignalDisposable : IDisposable
        {
            AsyncSemaphore _semaphore;

            public SignalDisposable(AsyncSemaphore semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                if (_semaphore == null) return;
                _semaphore.Signal();
                _semaphore = null;
            }
        }
    }
}
