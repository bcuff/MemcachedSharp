using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MemcachedSharp
{
    internal class PipelinedPool<T> : IPool<T>
    {
        readonly Func<Task<T>> _createFactory;
        readonly int _minItems;
        readonly int _maxItems;
        readonly int _maxRequestsPerItem;
        readonly List<Item> _items;
        readonly List<Item> _overflowItems;
        readonly AsyncSemaphore _semaphore;

        public PipelinedPool(Func<Task<T>> createFactory, PipelinedPoolOptions options = null)
        {
            if (createFactory == null) throw new ArgumentNullException("createFactory");

            if (options == null) options = new PipelinedPoolOptions();
            // min and max are intended to be small number e.g. 2 and 8
            // I wouldn't recommend using number greater than 50 or so
            // because most operations in the pool will be O(n)
            _minItems = options.MinTargetItemCount;
            _maxItems = options.MaxTargetItemCount;
            _maxRequestsPerItem = options.MaxRequestsPerItem;
            _items = new List<Item>(_minItems);
            _overflowItems = new List<Item>(_maxItems);
            _semaphore = new AsyncSemaphore(_maxItems * _maxRequestsPerItem);
        }

        public async Task<IPooledItem<T>> Borrow()
        {
            using (await _semaphore.WaitAndSignal())
            {
                Item result;
                lock (_items)
                {
                    for (int i = 0; i < _items.Count; ++i)
                    {
                        var item = _items[i];
                        if (item.CurrentRequestCount < _maxRequestsPerItem)
                        {
                            result = item;
                            Interlocked.Increment(ref item.CurrentRequestCount);
                            break;
                        }
                    }

                    if (_items.Count < _minItems)
                    {
                        var item = new Item(this, false);
                        _items.Add(item);
                        item.CurrentRequestCount = 1;
                        result = item;
                    }
                    else
                    {
                        for (int i = 0; i < _overflowItems.Count; ++i)
                        {
                        }
                    }
                }

            }
        }

        private void Return(Item item)
        {
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private class LoanerItem : IPooledItem<T>
        {
            Item _item;

            public LoanerItem(Item item)
            {
                _item = item;
            }

            public T Item
            {
                get
                {
                    var item = _item;
                    if (item == null) throw new ObjectDisposedException(GetType().Name);
                    return item.Value.Result;
                }
            }

            public bool IsCorrupted { get; set; }

            public void Dispose()
            {
                var item = _item;
                _item = null;
                item.Return();
            }
        }

        private class Item
        {
            readonly PipelinedPool<T> _owner;
            Task<T> _value;
            Exception _error;

            public Item(PipelinedPool<T> owner, bool isOverflow)
            {
                _owner = owner;
                IsOverflow = isOverflow;
            }

            public Task<T> Value
            {
                get
                {
                    if (_value != null) return _value;
                    lock (this)
                    {
                        if (_value != null) return _value;
                        if (_error != null) throw new PoolCreationException("An attempt to create an object of type=" + typeof(T).Name + " already failed.");
                        try
                        {
                            return _value = _owner._createFactory();
                        }
                        catch (Exception e)
                        {
                            _error = e;
                            throw new PoolCreationException("An attempt to create an object of type=" + typeof(T).Name + " failed.", e);
                        }
                    }
                }
            }

            public bool IsOverflow { get; set; }

            public void Return()
            {
                _owner.Return(this);
            }

            public int CurrentRequestCount;
        }
    }
}
