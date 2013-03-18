using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp
{
    interface IResponseReader
    {
        Task<ResponseLine> ReadLine(bool validate = false);
        Task<MemcachedItem> ReadItem();
    }
}
