using System;
using System.Linq;

namespace MemcachedSharp
{
    public class MemcachedStorageOptions
    {
        public uint Flags { get; set; }

        public DateTime? ExpirationTime { get; set; }

        internal string GetComandLine(string command, string key, long bytes)
        {
            var expires = ExpirationTime.HasValue ? ExpirationTime.Value.ToUnixTimestamp() : 0U;
            return string.Format("{0} {1} {2} {3} {4}\r\n", command, key, Flags, expires, bytes);
        }
    }
}
