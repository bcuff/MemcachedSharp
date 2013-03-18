using System;
using System.Linq;

namespace MemcachedSharp
{
    interface IPooledItem<out T> : IDisposable
    {
        T Item { get; }
        bool IsCorrupted { get; set; }
    }
}
