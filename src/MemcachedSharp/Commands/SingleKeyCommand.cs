using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp.Commands
{
    abstract class SingleKeyCommand<T> : ICommand<T>
    {
        public string Key { get; set; }
        public abstract Task SendRequest(ISocket socket);
        public abstract Task<T> ReadResponse(IResponseReader reader);
    }
}
