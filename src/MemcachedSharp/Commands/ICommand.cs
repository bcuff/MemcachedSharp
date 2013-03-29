using System;
using System.Threading.Tasks;

namespace MemcachedSharp.Commands
{
    internal interface ICommand
    {
        string Verb { get; }
    }

    internal interface ICommand<T> : ICommand
    {
        Task SendRequest(ISocket socket);
        Task<T> ReadResponse(IResponseReader reader);
    }
}
