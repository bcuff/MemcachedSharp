using System;
using System.Linq;

namespace MemcachedSharp
{
    public class MemcachedStorageOptions
    {
        public uint Flags { get; set; }

        public TimeSpan? ExpirationTime { get; set; }
    }
}
