using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MemcachedSharp
{
    /// <summary>
    /// Encapsulates a response object from Memcached.
    /// </summary>
    public sealed class MemcachedItem
    {
        internal MemcachedItem(string key, uint flags, long size, long? casUnique, MemoryStream data)
        {
            Key = key;
            Flags = flags;
            Size = size;
            CasUnique = casUnique;
            Data = data;
        }

        /// <summary>
        /// Gets the key of the object retrieved from Memcached;
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the flags value of the object retrieved from Memcached.
        /// </summary>
        public uint Flags { get; private set; }

        /// <summary>
        /// Gets the size of the object retrieved from Memcached.
        /// </summary>
        public long Size { get; private set; }

        /// <summary>
        /// Gets the cas unique field of the object retrieved from Memcached.
        /// </summary>
        public long? CasUnique { get; private set; }

        /// <summary>
        /// Gets a <c>Stream</c> of the data retrieved from Memcached.
        /// </summary>
        public Stream Data { get; private set; }
    }
}
