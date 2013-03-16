using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp
{
    public class MemcachedConnection : IDisposable
    {
        static readonly MemcachedStorageOptions _defaultStorageOptions = new MemcachedStorageOptions();
        static readonly byte[] _endLineBuffer = Encoding.UTF8.GetBytes("\r\n");

        readonly IPEndPoint _endPoint;
        readonly Socket _socket;
        NetworkStream _stream;
        MemcachedResponseReader _reader;

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
            _reader = new MemcachedResponseReader(_stream, Encoding.UTF8);
        }

        public async Task<MemcachedItem> Get(string key)
        {
            ValidateKey(key);
            EnsureOpen();

            var commandLine = ("get " + key + "\r\n").ToUtf8();
            await _stream.WriteAsync(commandLine, 0, commandLine.Length);

            var line = await _reader.ReadLine();
            var parts = line.Split(' ');
            ValidateResponse(line, parts);

            if (!parts[0].Equals("VALUE", StringComparison.OrdinalIgnoreCase))
            {
                throw new MemcachedException("Invalid response line - " + line);
            }

            if (parts.Length < 4) throw new MemcachedException("Invalid response line - " + line);

            var responseKey = parts[1];
            uint flags;
            int bytes;
            long? casUnique = null;

            if (!uint.TryParse(parts[2], out flags)) throw new MemcachedException("Invalid response line - " + line);
            if (!int.TryParse(parts[3], out bytes) || bytes < 0) throw new MemcachedException("Invalid response line - " + line);
            if (parts.Length > 4)
            {
                long temp;
                if (!long.TryParse(parts[4], out temp)) throw new MemcachedException("Invalid response line - " + line);
                casUnique = temp;
            }

            var body = await _reader.ReadBody(bytes);
            var endline = await _reader.ReadLine();
            if (endline.Length > 0) throw new MemcachedException("Unexpected line read - " + endline);

            endline = await _reader.ReadLine();
            if (endline != "END") throw new MemcachedException("Unexpected line read - " + endline);

            return new MemcachedItem(responseKey, flags, bytes, casUnique, body);
        }

        public async Task Set(string key, byte[] value, MemcachedStorageOptions options = null)
        {
            ValidateKey(key);
            EnsureOpen();

            if(options == null) options = _defaultStorageOptions;

            var commandLine = options.GetComandLine("set", key, value.Length).ToUtf8();
            var requestData = new [] {
                new ArraySegment<byte>(commandLine),
                new ArraySegment<byte>(value),
                new ArraySegment<byte>(_endLineBuffer),
            };
            _socket.SendAsync(requestData);

            var responseLine = await _reader.ReadLine();
            var parts = responseLine.Split(' ');
            ValidateResponse(responseLine, parts);

            if (parts[0] != "STORED")
            {
                throw new MemcachedException("Unexpected response line - " + responseLine);
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

        private void ValidateKey(string key)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (key.Length > 250) throw new ArgumentException("key must be no more than 250 characters in length", "key");
        }

        private void EnsureOpen()
        {
            if (_stream == null) throw new IOException("Connection isn't open");
        }

        public void Dispose()
        {
            _socket.Dispose();
            if(_stream != null) _stream.Dispose();
        }
    }
}
