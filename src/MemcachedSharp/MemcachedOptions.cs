using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemcachedSharp
{
    public class MemcachedOptions
    {
        public MemcachedOptions()
        {
            MaxConnections = 2;
            MaxConcurrentRequestPerConnection = 15;
            EnablePipelining = true;
        }

        public bool EnablePipelining { get; set; }
        public int MaxConcurrentRequestPerConnection { get; set; }
        public int MaxConnections { get; set; }
    }
}
