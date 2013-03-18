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
            MinTargetItemCount = 2;
            MaxTargetItemCount = 8;
            MaxRequestsPerItem = 10;
        }

        public int MinTargetItemCount { get; set; }
        public int MaxTargetItemCount { get; set; }
        public int MaxRequestsPerItem { get; set; }
    }
}
