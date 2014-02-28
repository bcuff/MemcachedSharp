﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemcachedSharp
{
    internal class Pool<T> : IPool<T>
    {
        readonly Stack<T> _items = new Stack<T>();
        readonly AsyncSemaphore _semaphore;
        readonly Func<Task<T>> _creationFactory;
        readonly Func<T, bool> _itemValidator;
        bool _disposed;

        public Pool(Func<Task<T>> creationFactory, Func<T, bool> itemValidator = null, PoolOptions options = null)
        {
            if (creationFactory == null) throw new ArgumentNullException("creationFactory");

            _creationFactory = creationFactory;
            _itemValidator = itemValidator ?? (v => true);
            if (options != null)
            {
                if (options.MaxCount > 0)
                {
                    _semaphore = new AsyncSemaphore(options.MaxCount);
                }
            }
        }

        public int Size
        {
            get { return _items.Count; }
        }

        public async Task<IPooledItem<T>> Borrow()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            if (_semaphore != null) await _semaphore.Wait();
            try
            {
                bool gotItem;
                T item = default(T);
                lock (_items)
                {
                    if (_disposed) throw new ObjectDisposedException(GetType().Name);
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

        public void Clear()
        {
            Clear(false);
        }

        public void Dispose()
        {
            Clear(true);
        }

        private void Clear(bool disposing)
        {
            lock (_items)
            {
                List<Exception> exceptions = null;
                while (_items.Count > 0)
                {
                    var item = _items.Pop();
                    var disposable = item as IDisposable;
                    if (disposable != null)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception e)
                        {
                            if (exceptions == null) exceptions = new List<Exception>();
                            exceptions.Add(e);
                        }
                    }
                }

                if (disposing)
                    _disposed = true;

                if (exceptions != null)
                {
                    throw new AggregateException(exceptions);
                }
            }
        }

        private void Return(T item, bool isCorrupted)
        {
            try
            {
                try
                {
                    isCorrupted = isCorrupted || !_itemValidator(item);
                }
                finally
                {
                    if (isCorrupted || _disposed)
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
