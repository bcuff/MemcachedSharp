using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuffsoft.Memcached
{
    public class MemcachedItem
    {
        internal MemcachedItem(string key, int flags, long size, long? casUnique, MemoryStream data)
        {
            Key = key;
            Flags = flags;
            Size = size;
            CasUnique = casUnique;
            Data = data;
        }

        public string Key { get; private set; }
        public int Flags { get; private set; }
        public long Size { get; private set; }
        public long? CasUnique { get; private set; }
        public Stream Data { get; private set; }
    }
}
