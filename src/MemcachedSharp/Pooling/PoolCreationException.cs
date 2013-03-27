using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MemcachedSharp
{
    /// <summary>
    /// Represents errors that happen when a pool fails to create an item.
    /// </summary>
    [Serializable]
    public sealed class PoolCreationException : Exception
    {
        internal PoolCreationException(string message) : base(message) { }
        internal PoolCreationException(string message, Exception inner) : base(message, inner) { }
        private PoolCreationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
