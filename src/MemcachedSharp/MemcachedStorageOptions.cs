using System;
using System.Linq;

namespace MemcachedSharp
{
    /// <summary>
    /// Encapsulates options for storage operations in Memcached.
    /// </summary>
    public sealed class MemcachedStorageOptions
    {
        /// <summary>
        /// Gets or sets the flags field on the object to store in Memcached.
        /// </summary>
        public uint Flags { get; set; }

        /// <summary>
        /// Gets or sets the expires field on the object to store in Memcached.
        /// </summary>
        public TimeSpan? ExpirationTime { get; set; }
    }
}
