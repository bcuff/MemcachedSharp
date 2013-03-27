using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MemcachedSharp
{
    /// <summary>
    /// Represents errors that occurr that are related to Memcached.
    /// </summary>
    [Serializable]
    public sealed class MemcachedException : Exception
    {
        private MemcachedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        internal MemcachedException(string message)
            : base(message)
        {
        }
    }
}
