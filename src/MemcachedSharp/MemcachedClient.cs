using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MemcachedSharp.Commands;

namespace MemcachedSharp
{
    public class MemcachedClient : IDisposable
    {
        readonly int _port;
        readonly string _host;
        readonly IPAddress _ip;
        readonly IPool<MemcachedConnection> _pool;

        public MemcachedClient(string endpoint, MemcachedOptions options = null)
        {
            if (endpoint == null) throw new ArgumentNullException("endpoint");
            var parts = endpoint.Split(new[] {':'}, StringSplitOptions.None);
            if (parts.Length == 0 || parts.Length > 2)
                throw new ArgumentException("Invalid endpoint parameter", "endpoint");
            if (parts.Length == 2)
            {
                if (!int.TryParse(parts[1], out _port))
                {
                    throw new ArgumentException("Invalid port number in endpoint parameter", "endpoint");
                }
            }
            else
            {
                _port = 11211;
            }
            _host = parts[0];
            IPAddress.TryParse(_host, out _ip);

            if (options == null) options = new MemcachedOptions();
            if (options.EnablePipelining)
            {
                _pool = new PipelinedPool<MemcachedConnection>(CreateConnection, new PipelinedPoolOptions
                {
                    TargetItemCount = options.MaxConnections,
                    MaxRequestsPerItem = options.MaxConcurrentRequestPerConnection,
                });
            }
            else
            {
                _pool = new Pool<MemcachedConnection>(CreateConnection, new PoolOptions
                {
                    MaxCount = options.MaxConnections,
                });
            }
        }

        /// <summary>
        /// Gets a <c>MemcachedItem</c> from memcached with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the item to get. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <returns>
        /// A task containing a <c>MemcachedItem</c> that encapsulates the resposne and data if found; othwerwise <c>null</c>.
        /// </returns>
        public Task<MemcachedItem> Get(string key)
        {
            Util.ValidateKey(key);
            return Execute(new GetCommand { Key = key, });
        }

        /// <summary>
        /// Stores the specified <paramref name="value"/> in memcached with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the item to set. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <param name="value">A <c>byte</c>[] containing the data to be stored in memcached.</param>
        /// <param name="options">Optional options to pass to memcached.</param>
        /// <returns>A task that completes when the operation finishes successfully or faults in the event of a failure.</returns>
        public Task Set(string key, byte[] value, MemcachedStorageOptions options = null)
        {
            Util.ValidateKey(key);
            if (value == null) throw new ArgumentNullException("value");
            return InternalSet(key, value, 0, value.Length, options);
        }

        /// <summary>
        /// Stores the specified data in memcached with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the item to set. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <param name="buffer">A <c>byte</c>[] containing the data to be stored in memcached.</param>
        /// <param name="offset">The point in <paramref name="buffer"/> at which the data to store begins.</param>
        /// <param name="count">The number of bytes in <paramref name="buffer"/> to store.</param>
        /// <param name="options">Optional options to pass to memcached.</param>
        /// <returns>A task that completes when the operation finishes successfully or faults in the event of a failure.</returns>
        public Task Set(string key, byte[] buffer, int offset, int count, MemcachedStorageOptions options = null)
        {
            Util.ValidateKey(key);
            Util.ValidateBuffer(buffer, offset, count);
            return InternalSet(key, buffer, offset, count, options);
        }

        internal async Task InternalSet(string key, byte[] buffer, int offset, int count, MemcachedStorageOptions options = null)
        {
            var command = new SetCommand
            {
                Key = key,
                Data = new ArraySegment<byte>(buffer, offset, count),
                Options = options,
            };
            var result = await Execute(command);
            if (result != StorageCommandResult.Stored)
            {
                throw Util.CreateUnexpectedStorageResponse(StorageCommandResult.Stored, result);
            }
        }

        /// <summary>
        /// Deletes a value with the specified <paramref name="key"/> from memcached.
        /// </summary>
        /// <param name="key">The key of the item to delete. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <returns>A task that completes when the operation finishes successfully or faults in the event of a failure.</returns>
        public Task<bool> Delete(string key)
        {
            return Execute(new DeleteCommand { Key = key });
        }

        private async Task<T> Execute<T>(ICommand<T> command)
        {
            using (var conn = await _pool.Borrow())
            {
                try
                {
                    return await conn.Item.Execute(command);
                }
                catch
                {
                    conn.IsCorrupted = true;
                    throw;
                }
            }
        }

        private async Task<MemcachedConnection> CreateConnection()
        {
            IPAddress address = _ip;
            if (address == null)
            {
                var addresses = await DnsExtensions.GetHostAddressesAsync(_host);
                address = addresses.First();
            }
            var endpoint = new IPEndPoint(address, _port);
            var conn = new MemcachedConnection(endpoint);
            await conn.Open();
            return conn;
        }

        /// <summary>
        /// Closes all connections to memcached and cleans up any other resources that may be in use.
        /// </summary>
        public void Dispose()
        {
            _pool.Dispose();
        }
    }
}
