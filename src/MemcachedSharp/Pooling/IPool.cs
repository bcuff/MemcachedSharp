using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp
{
    internal interface IPool<T> : IDisposable
    {
        int Size { get; }
        Task<IPooledItem<T>> Borrow();
        void Clear();
    }
}
