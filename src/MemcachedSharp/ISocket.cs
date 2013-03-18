using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp
{
    interface ISocket
    {
        Task SendAsync(byte[] buffer, int offset, int count, SocketFlags flags = SocketFlags.None);
        Task SendAsync(IList<ArraySegment<byte>> buffers, SocketFlags flags = SocketFlags.None);
    }
}
