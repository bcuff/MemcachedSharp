using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp.Test
{
    internal class MockSocket : ISocket
    {
        readonly MemoryStream _dataSent = new MemoryStream();

        #region ISocket Members

        public Task SendAsync(byte[] buffer, int offset, int count, SocketFlags flags = SocketFlags.None)
        {
            _dataSent.Write(buffer, offset, count);
            var result = new TaskCompletionSource<int>();
            result.SetResult(1);
            return result.Task;
        }

        public async Task SendAsync(IList<ArraySegment<byte>> buffers, SocketFlags flags = SocketFlags.None)
        {
            foreach (var buffer in buffers)
            {
                await SendAsync(buffer.Array, buffer.Offset, buffer.Count, flags);
            }
        }

        #endregion

        public Stream GetSentData()
        {
            var data = _dataSent.ToArray();
            return new MemoryStream(data);
        }
    }
}
