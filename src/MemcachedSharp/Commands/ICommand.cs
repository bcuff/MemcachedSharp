using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp.Commands
{
    interface ICommand<T>
    {
        Task SendRequest(ISocket socket);
        Task<T> ReadResponse(IResponseReader reader);
    }
}
