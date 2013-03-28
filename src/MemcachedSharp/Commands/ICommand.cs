using System;
using System.Threading.Tasks;

namespace MemcachedSharp.Commands
{
    interface ICommand<T>
    {
        Task SendRequest(ISocket socket);
        Task<T> ReadResponse(IResponseReader reader);
    }
}
