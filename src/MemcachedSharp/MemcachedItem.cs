using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp
{
    public class MemcachedItem
    {
        internal MemcachedItem(string key, uint flags, long size, long? casUnique, MemoryStream data)
        {
            Key = key;
            Flags = flags;
            Size = size;
            CasUnique = casUnique;
            Data = data;
        }

        public string Key { get; private set; }
        public uint Flags { get; private set; }
        public long Size { get; private set; }
        public long? CasUnique { get; private set; }
        public Stream Data { get; private set; }
    }
}
