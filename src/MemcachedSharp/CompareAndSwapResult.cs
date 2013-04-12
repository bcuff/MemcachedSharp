using System;

namespace MemcachedSharp
{
    /// <summary>
    /// Represents the result of a Memcached 'cas' operation. Possible results are Success, 
    /// </summary>
    public enum CasResult
    {
        /// <summary>
        /// The item was stored because the specified cas unique field matched the one in Memcached.
        /// </summary>
        Stored,
        /// <summary>
        /// The item was not stored because the specified cas unique field didn't match the one in Memcached.
        /// </summary>
        Exists,
        /// <summary>
        /// No operation was performed because no item exists for the specified key.
        /// </summary>
        NotFound,
    }
}
