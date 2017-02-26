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
    /// Represents errors that happen when a pool fails to create an item.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public sealed class PoolCreationException : Exception
    {
        internal PoolCreationException(string message) : base(message) { }
        internal PoolCreationException(string message, Exception inner) : base(message, inner) { }
#if NET45
        private PoolCreationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }
}
