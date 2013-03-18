using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MemcachedSharp.Commands;

namespace MemcachedSharp
{
    internal class MemcachedConnection : IDisposable
    {
        readonly object _syncRoot = new object();
        readonly IPEndPoint _endPoint;
        readonly AsyncSemaphore _responseSemaphore = new AsyncSemaphore(1);
        Socket _socket;
        ISocket _isocket;
        NetworkStream _stream;
        MemcachedResponseReader _reader;
        MemcachedConnectionState _state;
        bool _disposed;

        public MemcachedConnection(IPEndPoint endPoint)
        {
            if (endPoint == null) throw new ArgumentNullException("endPoint");

            _endPoint = endPoint;
            _socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true
            };
        }

        public async Task<T> Execute<T>(ICommand<T> command)
        {
            Task sendTask;
            Task<IDisposable> responseSemaphoreTask;
            lock (_syncRoot)
            {
                EnsureOpen();
                sendTask = command.SendRequest(_isocket);
                responseSemaphoreTask = _responseSemaphore.WaitAndSignal();
            }
            try
            {
                await sendTask;
            }
            catch
            {
                lock (_syncRoot)
                {
                    SetFaulted();
                }
#pragma warning disable 4014
                responseSemaphoreTask.ContinueWith(t => t.Result.Dispose());
#pragma warning restore 4014
                throw;
            }
            using (await responseSemaphoreTask)
            {
                lock (_syncRoot) EnsureOpen();
                return await command.ReadResponse(_reader);
            }
        }

        private void SetFaulted()
        {
            _state = MemcachedConnectionState.Faulted;
            Dispose();
        }

        public async Task Open()
        {
            if (_state != MemcachedConnectionState.UnOpened) throw new InvalidOperationException("This connection has already been opened.");

            Task connectTask;
            lock (_syncRoot)
            {
                try
                {
                    connectTask = _socket.ConnectAsync(_endPoint);
                    _state = MemcachedConnectionState.Opening;
                }
                catch
                {
                    SetFaulted();
                    throw;
                }
            }

            try
            {
                await connectTask;
            }
            catch
            {
                lock (_syncRoot) SetFaulted();
                throw;
            }
            lock (_syncRoot)
            {
                _stream = new NetworkStream(_socket);
                _reader = new MemcachedResponseReader(_stream, Encoding.UTF8);
                _isocket = new MemcachedSocket(_socket);
                _state = MemcachedConnectionState.Open;
            }
        }

        private void EnsureOpen()
        {
            if (_state != MemcachedConnectionState.Open) throw new IOException("Connection isn't open. Current state=" + _state);
        }

        public void Dispose()
        {
            Socket socket;
            Stream stream;
            lock (_syncRoot)
            {
                if (_disposed) return;
                if (_state != MemcachedConnectionState.Faulted)
                {
                    _state = MemcachedConnectionState.Closed;
                }
                socket = _socket;
                _socket = null;
                stream = _stream;
                _stream = null;
                _isocket = null;
                _reader = null;
                _disposed = true;
            }
            try
            {
                socket.Dispose();
            }
            finally
            {
                if (stream != null) stream.Dispose();
            }
        }
    }
}
