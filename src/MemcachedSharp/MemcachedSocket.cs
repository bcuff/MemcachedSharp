using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp
{
    class MemcachedSocket : ISocket
    {
        readonly Socket _socket;

        public MemcachedSocket(Socket socket)
        {
            _socket = socket;
        }

        public Task SendAsync(byte[] buffer, int offset, int count, SocketFlags flags = SocketFlags.None)
        {
#if NET45
            return _socket.SendAsync(buffer, offset, count, flags);
#else
            return _socket.SendAsync(new ArraySegment<byte>(buffer, offset, count), flags);
#endif
        }

        public Task SendAsync(IList<ArraySegment<byte>> buffers, SocketFlags flags = SocketFlags.None)
        {
            return _socket.SendAsync(buffers, flags);
        }
    }
}
