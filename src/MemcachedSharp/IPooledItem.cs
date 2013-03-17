using System;
using System.Linq;

namespace MemcachedSharp
{
    interface IPooledItem<T> : IDisposable
    {
        T Item { get; }
        bool IsCorrupted { get; set; }
    }
}
