using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp.Commands
{
    abstract class StorageCommand<T> : SingleKeyCommand<T>
    {
        static readonly byte[] _endLineBuffer = Encoding.UTF8.GetBytes("\r\n");

        public MemcachedStorageOptions Options { get; set; }
        public ArraySegment<byte> Data { get; set; }
        public abstract string Verb { get; }

        public sealed override Task SendRequest(ISocket socket)
        {
            uint flags = 0;
            uint expires = 0;
            if (Options != null)
            {
                flags = Options.Flags;
                if (Options.ExpirationTime.HasValue)
                {
                    expires = (uint)Options.ExpirationTime.Value.TotalSeconds;
                }
            }
            var line = string.Format("{0} {1} {2} {3} {4}\r\n", Verb, Key, flags, expires, Data.Count);
            return socket.SendAsync(new[]
            {
                new ArraySegment<byte>(line.ToUtf8()),
                Data,
                new ArraySegment<byte>(_endLineBuffer),
            });
        }

        public abstract override Task<T> ReadResponse(IResponseReader reader);
    }
}
