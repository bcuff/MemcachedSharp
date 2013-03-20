using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemcachedSharp
{
    class PipelinedPoolOptions
    {
        public PipelinedPoolOptions()
        {
            TargetItemCount = 2;
            MaxRequestsPerItem = 10;
        }

        public int TargetItemCount { get; set; }
        public int MaxRequestsPerItem { get; set; }
    }
}
