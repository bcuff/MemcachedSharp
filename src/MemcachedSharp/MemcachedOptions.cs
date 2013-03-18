using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemcachedSharp
{
    public class MemcachedOptions
    {
        public MemcachedOptions()
        {
            MaxConnections = 2;
        }

        public int MaxConnections { get; set; }
    }
}
