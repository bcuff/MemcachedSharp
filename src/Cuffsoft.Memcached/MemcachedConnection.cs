using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Cuffsoft.Memcached
{
    public class MemcachedConnection
    {
        readonly IPEndPoint _endPoint;
        readonly Socket _socket;
        NetworkStream _stream;

        public MemcachedConnection(IPEndPoint endPoint)
        {
            if (endPoint == null) throw new ArgumentNullException("endPoint");

            _endPoint = endPoint;
            _socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true
            };
        }

        public async Task Open()
        {
            if (_stream != null) throw new InvalidOperationException("Already connected.");

            await _socket.ConnectAsync(_endPoint);
            _stream = new NetworkStream(_socket);
        }

        public async Task<Stream> Get(string key)
        {
            if (key == null) throw new ArgumentNullException("key");

            var commandLine = ("get " + key + "\r\n").ToUtf8();
            await _stream.WriteAsync(commandLine, 0, commandLine.Length);

            var reader = new MemcachedResponseReader(_stream, Encoding.UTF8);
            var line = await reader.ReadLine();

            var parts = line.Split(' ');

            ValidateResponse(line, parts);

            if (!parts[0].Equals("VALUE", StringComparison.OrdinalIgnoreCase))
            {
                if (parts.Length < 4) throw new MemcachedException("Invalid response line - " + line);

                var key = parts[1];
                int flags;
                long bytes, casUnique;

                if (!int.TryParse(parts[2], out flags)) throw new MemcachedException("Invalid response line - " + line);
                if (!long.TryParse(parts[3], out bytes)) throw new MemcachedException("Invalid response line - " + line);
                if (parts.Length > 4)
                {
                    if (!long.TryParse(parts[4], out casu)) throw new MemcachedException("Invalid response line - " + line);
                }
            }
        }

        static readonly HashSet<string> _errorResponses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ERROR",
            "CLIENT_ERROR",
            "SERVER_ERROR",
        };

        private void ValidateResponse(string line, string[] lineParts)
        {
            if (lineParts.Length == 0 || _errorResponses.Contains(lineParts[0]))
            {
                throw new MemcachedException(line);
            }
        }
    }
}
