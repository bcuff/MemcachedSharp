using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemcachedSharp
{
    internal class Pool<T>
    {
        readonly Stack<T> _items = new Stack<T>();
        readonly AsyncSemaphore _semaphore;
        readonly Func<Task<T>> _creationFactory;

        public Pool(Func<Task<T>> creationFactory, PoolOptions options = null)
        {
            if (creationFactory == null) throw new ArgumentNullException("creationFactory");

            _creationFactory = creationFactory;
            if (options != null)
            {
                if (options.MaxCount > 0)
                {
                    _semaphore = new AsyncSemaphore(options.MaxCount);
                }
            }
        }

        public async Task<IPooledItem<T>> Borrow()
        {
            if (_semaphore != null) await _semaphore.Wait();
            try
            {
                bool gotItem;
                T item = default(T);
                lock (_items)
                {
                    gotItem = _items.Count > 0;
                    if (gotItem) item = _items.Pop();
                }
                if (!gotItem)
                {
                    item = await _creationFactory();
                }
                return new PooledItem(this, item);
            }
            catch
            {
                if(_semaphore != null) _semaphore.Signal();
                throw;
            }
        }

        private void Return(T item, bool isCorrupted)
        {
            try
            {
                if (isCorrupted)
                {
                    var disposable = item as IDisposable;
                    if (disposable != null) disposable.Dispose();
                }
                else
                {
                    lock (_items)
                    {
                        _items.Push(item);
                    }
                }
            }
            finally
            {
                if (_semaphore != null) _semaphore.Signal();
            }
        }

        private class PooledItem : IPooledItem<T>
        {
            readonly Pool<T> _owner;
            readonly T _item;
            bool _disposed;

            public PooledItem(Pool<T> owner, T item)
            {
                _owner = owner;
                _item = item;
            }

            public T Item
            {
                get { return _item; }
            }

            public bool IsCorrupted { get; set; }

            public void Dispose()
            {
                if (_disposed) return;
                _owner.Return(_item, IsCorrupted);
                _disposed = true;
            }
        }
    }
}
