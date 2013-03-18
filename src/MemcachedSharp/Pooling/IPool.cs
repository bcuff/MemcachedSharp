using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp
{
    internal interface IPool<T> : IDisposable
    {
        Task<IPooledItem<T>> Borrow();
    }
}
