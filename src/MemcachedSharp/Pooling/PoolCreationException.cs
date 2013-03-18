using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MemcachedSharp
{
    [Serializable]
    public sealed class PoolCreationException : Exception
    {
        public PoolCreationException(string message) : base(message) { }
        public PoolCreationException(string message, Exception inner) : base(message, inner) { }
        private PoolCreationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
