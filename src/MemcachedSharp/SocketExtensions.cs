﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp
{
    internal static class SocketExtensions
    {
        internal static Task SendAsync(this ISocket socket, byte[] data, SocketFlags flags = SocketFlags.None)
        {
            return socket.SendAsync(data, 0, data.Length, flags);
        }

#if NET45
        public static Task ConnectAsync(this Socket socket, EndPoint endPoint)
        {
            if (socket == null) throw new ArgumentNullException(nameof(socket));
            if (endPoint == null) throw new ArgumentNullException(nameof(endPoint));

            return Task.Factory.FromAsync(
                (ac, state) => socket.BeginConnect(endPoint, ac, state),
                socket.EndConnect,
                null,
                TaskCreationOptions.None);
        }

        public static Task SendAsync(this Socket socket, byte[] buffer, int offset, int size, SocketFlags flags = SocketFlags.None)
        {
            if (socket == null) throw new ArgumentNullException(nameof(socket));
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            return Task<int>.Factory.FromAsync(
                (ac, state) => socket.BeginSend(buffer, offset, size, flags, ac, state),
                socket.EndSend,
                null,
                TaskCreationOptions.None);
        }

        public static Task SendAsync(this Socket socket, byte[] buffer, SocketFlags flags = SocketFlags.None)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            return socket.SendAsync(buffer, 0, buffer.Length, flags);
        }

        public static Task<int> SendAsync(this Socket socket, IList<ArraySegment<byte>> buffers, SocketFlags flags = SocketFlags.None)
        {
            if (socket == null) throw new ArgumentNullException(nameof(socket));
            if (buffers == null) throw new ArgumentNullException(nameof(buffers));

            return Task<int>.Factory.FromAsync(
                (ac, state) => socket.BeginSend(buffers, flags, ac, state),
                socket.EndSend,
                null,
                TaskCreationOptions.None);
        }

        public static Task<int> ReceiveAsync(this Socket socket, byte[] buffer, int offset, int size, SocketFlags flags = SocketFlags.None)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            return Task<int>.Factory.FromAsync(
                (ac, state) => socket.BeginReceive(buffer, offset, size, flags, ac, state),
                socket.EndReceive,
                null,
                TaskCreationOptions.None);
        }
#endif
    }
}
