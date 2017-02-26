using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if NET45
using System.Runtime.Serialization;
#endif

namespace MemcachedSharp
{
    /// <summary>
    /// Represents errors that occur that are related to Memcached.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public sealed class MemcachedException : Exception
    {
#if NET45
        private MemcachedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif

        internal MemcachedException(string message)
            : base(message)
        {
        }
    }
}
