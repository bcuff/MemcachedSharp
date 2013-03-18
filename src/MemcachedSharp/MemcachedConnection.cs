using System;
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

        public Task<MemcachedItem> Get(string key)
        {
            Util.ValidateKey(key);
            return InternalGet(key);
        }

        internal async Task<MemcachedItem> InternalGet(string key)
        {
            EnsureOpen();

            var commandLine = ("get " + key + "\r\n").ToUtf8();
            await _socket.SendAsync(commandLine, 0, commandLine.Length);

            var result = await _reader.ReadItem();

            if (result != null)
            {
                var nextItem = await _reader.ReadItem();
                if (nextItem != null)
                {
                    throw new MemcachedException("Memcached returned more items than expected.");
                }
            }

            return result;
        }

        public Task Set(string key, byte[] value, MemcachedStorageOptions options = null)
        {
            Util.ValidateKey(key);
            if (value == null) throw new ArgumentNullException("value");
            return InternalSet(key, value, 0, value.Length, options);
        }

        public Task Set(string key, byte[] buffer, int offset, int count, MemcachedStorageOptions options = null)
        {
            Util.ValidateKey(key);
            Util.ValidateBuffer(buffer, offset, count);
            return InternalSet(key, buffer, offset, count, options);
        }

        internal async Task InternalSet(string key, byte[] value, int offset, int count, MemcachedStorageOptions options = null)
        {
            EnsureOpen();

            if(options == null) options = _defaultStorageOptions;

            var commandLine = options.GetComandLine("set", key, value.Length).ToUtf8();
            var requestData = new [] {
                new ArraySegment<byte>(commandLine),
                new ArraySegment<byte>(value, offset, count),
                new ArraySegment<byte>(_endLineBuffer),
            };
            await _socket.SendAsync(requestData);

            var line = await _reader.ReadLine();
            if (line.Parts[0] != "STORED")
            {
                throw new MemcachedException("Unexpected response line - " + line);
            }
        }

        public Task<bool> Delete(string key)
        {
            Util.ValidateKey(key);

            return InternalDelete(key);
        }

        internal async Task<bool> InternalDelete(string key)
        {
            EnsureOpen();
            var commandLine = string.Format("delete {0}\r\n", key);
            await _socket.SendAsync(commandLine.ToUtf8());

            var responseLine = await _reader.ReadLine();
            if (responseLine.Parts[0] == "DELETED") return true;
            if (responseLine.Parts[0] == "NOT_FOUND") return false;
            throw new MemcachedException("Unexpected response line - " + responseLine);
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
