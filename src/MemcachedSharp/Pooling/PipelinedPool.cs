using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp
{
    internal class PipelinedPool<T> : IPool<T>
    {
        readonly Func<Task<T>> _createFactory;
        readonly int _targetItemCount;
        readonly int _maxRequestsPerItem;
        readonly List<ResidentItem> _items;
        readonly LinkedList<TaskCompletionSource<ResidentItem>> _waiters;
        bool _disposed;

        public PipelinedPool(Func<Task<T>> createFactory, PipelinedPoolOptions options = null)
        {
            if (createFactory == null) throw new ArgumentNullException("createFactory");

            _createFactory = createFactory;

            if (options == null) options = new PipelinedPoolOptions();
            // target item count should be relatively small like 2 - 8
            // I wouldn't recommend using number greater than 50 or so
            // because most operations in the pool will be O(n)
            _targetItemCount = options.TargetItemCount;
            _maxRequestsPerItem = options.MaxRequestsPerItem;
            _items = new List<ResidentItem>();
            _waiters = new LinkedList<TaskCompletionSource<ResidentItem>>();
        }

        private ResidentItem TryGetItem()
        {
            ResidentItem result = null;
            for (int i = 0; i < _items.Count; ++i)
            {
                var item = _items[i];
                if (item.CurrentRequestCount == 0)
                {
                    result = item;
                    break;
                }
                if (result == null)
                {
                    if (item.CurrentRequestCount < _maxRequestsPerItem)
                    {
                        result = item;
                    }
                }
                else if (item.CurrentRequestCount < result.CurrentRequestCount)
                {
                    result = item;
                }
            }
            if (result != null)
            {
                result.CurrentRequestCount++;
            }
            else if (_items.Count < _targetItemCount)
            {
                result = new ResidentItem(this) { CurrentRequestCount = 1 };
                _items.Add(result);
            }
            return result;
        }

        public async Task<IPooledItem<T>> Borrow()
        {
            TaskCompletionSource<ResidentItem> waitItem = null;
            ResidentItem result = null;
            lock (_items)
            {
                if (_disposed) throw new ObjectDisposedException(GetType().Name);
                result = TryGetItem();
                if(result == null)
                {
                    waitItem = new TaskCompletionSource<ResidentItem>();
                    _waiters.AddLast(waitItem);
                }
            }
            if (result == null)
            {
                result = await waitItem.Task;
            }
            try
            {
                await result.EnsureValueCreated();
            }
            catch
            {
                result.Return(true);
                throw;
            }
            return new LoanerItem(result);
        }

        public void Dispose()
        {
            ResidentItem[] itemsToDispose;
            ObjectDisposedException waiterException = null;
            lock (_items)
            {
                itemsToDispose = new ResidentItem[_items.Count];
                _items.CopyTo(itemsToDispose);
                foreach (var item in _items)
                {
                    item.CurrentRequestCount++;
                }
                _items.Clear();
                while (_waiters.First != null)
                {
                    if(waiterException == null) waiterException = new ObjectDisposedException(GetType().Name);
                    _waiters.First.Value.SetException(waiterException);
                    _waiters.RemoveFirst();
                }
                _disposed = true;
            }
            var exceptions = new List<Exception>();
            foreach (var item in itemsToDispose)
            {
                try
                {
                    item.Return(true);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }
            if (exceptions.Count > 0) throw new AggregateException(exceptions);
        }

        private class LoanerItem : IPooledItem<T>
        {
            ResidentItem _item;

            public LoanerItem(ResidentItem item)
            {
                _item = item;
            }

            public T Item
            {
                get
                {
                    var item = _item;
                    if (item == null) throw new ObjectDisposedException(GetType().Name);
                    return item.Value;
                }
            }

            public bool IsCorrupted { get; set; }

            public void Dispose()
            {
                var item = _item;
                _item = null;
                item.Return(IsCorrupted);
            }
        }

        private class ResidentItem : IDisposable
        {
            readonly PipelinedPool<T> _owner;
            Task _createValueTask;
            Exception _createError;
            bool _disposed;
            bool _removed;

            public ResidentItem(PipelinedPool<T> owner)
            {
                _owner = owner;
            }

            public T Value { get; private set; }

            public Task EnsureValueCreated()
            {
                if (_createValueTask != null) return _createValueTask;
                lock (this)
                {
                    if (_createValueTask != null) return _createValueTask;
                    _createValueTask = CreateValue();
                }
                return _createValueTask;
            }

            private async Task CreateValue()
            {
                if (_createError != null)
                {
                    throw new PoolCreationException("Failed to create pooled " + typeof(T).Name, _createError);
                }
                try
                {
                    Value = await _owner._createFactory();
                }
                catch (Exception e)
                {
                    _createError = e;
                    throw new PoolCreationException("Failed to create pooled " + typeof (T).Name, _createError);
                }
            }

            public void Return(bool remove)
            {
                bool dispose = false;
                TaskCompletionSource<ResidentItem> waiter = null;
                ResidentItem waiterResult = null;
                lock (_owner._items)
                {
                    if (!remove && !_removed && _owner._waiters.First != null)
                    {
                        waiter = _owner._waiters.First.Value;
                        waiterResult = this;
                        _owner._waiters.RemoveFirst();
                    }
                    else
                    {
                        if (--CurrentRequestCount < 0)
                        {
                            throw new InvalidOperationException(
                                "Items can't be removed more than the number of times they were borrowed.");
                        }
                        if (!_removed && remove)
                        {
                            _owner._items.Remove(this);
                            _removed = true;
                        }
                        // return true if the caller should dispose this item
                        dispose = CurrentRequestCount == 0 && _removed;
                        if (_owner._waiters.First != null)
                        {
                            waiterResult = _owner.TryGetItem();
                            Debug.Assert(waiterResult != null, "This shouldn't really happen");
                            if (waiterResult != null)
                            {
                                waiter = _owner._waiters.First.Value;
                                _owner._waiters.RemoveFirst();
                            }
                        }
                    }
                }
                if (waiter != null)
                {
                    waiter.SetResult(waiterResult);
                }
                else if (dispose)
                {
                    Dispose();
                }
            }

            // should be accessed within a lock
            public int CurrentRequestCount { get; set; }

            public void Dispose()
            {
                if (_disposed) return;
                T value;
                lock (this)
                {
                    if (_disposed) return;
                    value = Value;
                    _disposed = true;
                }
                var disposable = value as IDisposable;
                if (disposable != null) disposable.Dispose();
            }
        }
    }
}
