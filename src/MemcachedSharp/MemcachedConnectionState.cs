using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp
{
    enum MemcachedConnectionState
    {
        UnOpened,
        Opening,
        Open,
        Faulted,
        Closed,
    }
}
