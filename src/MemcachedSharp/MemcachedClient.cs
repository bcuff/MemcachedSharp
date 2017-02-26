using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MemcachedSharp.Commands;

namespace MemcachedSharp
{
    /// <summary>
    /// Encapsulates a client that may perform operations against a specified Memcached host.
    /// </summary>
    /// <remarks>
    /// Instances are stateful and internally pool connections to the specified host.
    /// Instances of this class ought to be disposed when they are no longer needed.
    /// </remarks>
    public sealed class MemcachedClient : IDisposable
    {
        readonly int _port;
        readonly string _host;
        readonly TimeSpan _connectTimeout;
        readonly TimeSpan _receiveTimeout;
        readonly IPAddress _ip;
        readonly IPool<MemcachedConnection> _pool;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemcachedClient"/> class.
        /// </summary>
        /// <param name="endpoint">
        ///     <para>The host name or IP address and, optionally, the port number of the target Memcached server.</para>
        ///     <para>Examples: "localhost:11211", "127.0.0.1"</para>
        /// </param>
        /// <param name="options">Optional. A set of options for the client.</param>
        /// <exception cref="ArgumentNullException">
        ///     <para><paramref name="endpoint"/> is <c>null</c>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <para><paramref name="endpoint"/> is invalid.</para>
        /// </exception>
        public MemcachedClient(string endpoint, MemcachedOptions options = null)
        {
            if (endpoint == null) throw new ArgumentNullException("endpoint");
            var parts = endpoint.Split(new[] { ':' }, StringSplitOptions.None);
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

            if (options == null)
            {
                options = new MemcachedOptions();
            }
            else
            {
                options.Validate("options");
            }

            _connectTimeout = options.ConnectTimeout;
            _receiveTimeout = options.ReceiveTimeout;

            if (options.EnablePipelining)
            {
                _pool = new PipelinedPool<MemcachedConnection>(CreateConnection, ValidateConnection, new PipelinedPoolOptions
                {
                    TargetItemCount = options.MaxConnections,
                    MaxRequestsPerItem = options.MaxConcurrentRequestPerConnection,
                });
            }
            else
            {
                _pool = new Pool<MemcachedConnection>(CreateConnection, ValidateConnection, new PoolOptions
                {
                    MaxCount = options.MaxConnections,
                });
            }
        }

        /// <summary>
        /// Gets a <c>MemcachedItem</c> from Memcached with the specified <paramref name="key"/>. Doesn't include the <see cref="MemcachedItem.CasUnique"/> field.
        /// </summary>
        /// <param name="key">The key of the item to get. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <returns>
        /// A task containing a <c>MemcachedItem</c> that encapsulates the response and data if found; otherwise <c>null</c>.
        /// </returns>
        public Task<MemcachedItem> Get(string key)
        {
            Util.ValidateKey(key);
            return Execute(new GetCommand { Key = key, });
        }

        /// <summary>
        /// Gets a <c>MemcachedItem</c> from Memcached with the specified <paramref name="key"/>. Includes the <see cref="MemcachedItem.CasUnique"/> field.
        /// </summary>
        /// <param name="key">The key of the item to get. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <returns>
        /// A task containing a <c>MemcachedItem</c> that encapsulates the response and data if found; otherwise <c>null</c>.
        /// </returns>
        public Task<MemcachedItem> Gets(string key)
        {
            Util.ValidateKey(key);
            return Execute(new GetsCommand { Key = key, });
        }

        /// <summary>
        /// Stores the specified <paramref name="value"/> in Memcached with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the item to set. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <param name="value">A <c>byte</c>[] containing the data to be stored in Memcached.</param>
        /// <param name="options">Optional options to pass to Memcached.</param>
        /// <returns>A task that completes when the operation finishes successfully or faults in the event of a failure.</returns>
        public Task Set(string key, byte[] value, MemcachedStorageOptions options = null)
        {
            Util.ValidateKey(key);
            if (value == null) throw new ArgumentNullException("value");
            return ExecuteStoreCommand<SetCommand, bool>(key, value, 0, value.Length, options);
        }

        /// <summary>
        /// Stores the specified data in Memcached with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the item to set. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <param name="buffer">A <c>byte</c>[] containing the data to be stored in Memcached.</param>
        /// <param name="offset">The point in <paramref name="buffer"/> at which the data to store begins.</param>
        /// <param name="count">The number of bytes in <paramref name="buffer"/> to store.</param>
        /// <param name="options">Optional options to pass to Memcached.</param>
        /// <returns>A task that completes when the operation finishes successfully or faults in the event of a failure.</returns>
        public Task Set(string key, byte[] buffer, int offset, int count, MemcachedStorageOptions options = null)
        {
            Util.ValidateKey(key);
            Util.ValidateBuffer(buffer, offset, count);
            return ExecuteStoreCommand<SetCommand, bool>(key, buffer, offset, count, options);
        }

        /// <summary>
        /// Adds the specified <paramref name="value"/> to Memcached if no value exists with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the item to set. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <param name="value">A <c>byte</c>[] containing the data to be stored in Memcached.</param>
        /// <param name="options">Optional options to pass to Memcached.</param>
        /// <returns>A task that completes with <c>true</c> if the item was added, <c>false</c> if an item already exists for that key, or completes unsuccessfully otherwise.</returns>
        public Task<bool> Add(string key, byte[] value, MemcachedStorageOptions options = null)
        {
            Util.ValidateKey(key);
            if (value == null) throw new ArgumentNullException("value");
            return ExecuteStoreCommand<AddCommand, bool>(key, value, 0, value.Length, options);
        }

        /// <summary>
        /// Adds the specified data to Memcached if no value exists with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the item to set. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <param name="buffer">A <c>byte</c>[] containing the data to be stored in Memcached.</param>
        /// <param name="offset">The point in <paramref name="buffer"/> at which the data to store begins.</param>
        /// <param name="count">The number of bytes in <paramref name="buffer"/> to store.</param>
        /// <param name="options">Optional options to pass to Memcached.</param>
        /// <returns>A task that completes with <c>true</c> if the item was added, <c>false</c> if an item already exists for that key, or completes unsuccessfully otherwise.</returns>
        public Task<bool> Add(string key, byte[] buffer, int offset, int count, MemcachedStorageOptions options = null)
        {
            Util.ValidateKey(key);
            Util.ValidateBuffer(buffer, offset, count);
            return ExecuteStoreCommand<AddCommand, bool>(key, buffer, offset, count, options);
        }

        /// <summary>
        /// Replaces the specified <paramref name="value"/> in Memcached if a value exists for the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the item to replace. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <param name="value">A <c>byte</c>[] containing the data to be stored in Memcached.</param>
        /// <param name="options">Optional options to pass to Memcached.</param>
        /// <returns>A task that completes with <c>true</c> if an item was replaced, <c>false</c> if no item existed for that key, or completes unsuccessfully otherwise.</returns>
        public Task<bool> Replace(string key, byte[] value, MemcachedStorageOptions options = null)
        {
            Util.ValidateKey(key);
            if (value == null) throw new ArgumentNullException("value");
            return ExecuteStoreCommand<ReplaceCommand, bool>(key, value, 0, value.Length, options);
        }

        /// <summary>
        /// Replaces the specified data in Memcached if a value exists for the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the item to replace. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <param name="buffer">A <c>byte</c>[] containing the data to be stored in Memcached.</param>
        /// <param name="offset">The point in <paramref name="buffer"/> at which the data to store begins.</param>
        /// <param name="count">The number of bytes in <paramref name="buffer"/> to store.</param>
        /// <param name="options">Optional options to pass to Memcached.</param>
        /// <returns>A task that completes with <c>true</c> if an item was replaced, <c>false</c> if no item existed for that key, or completes unsuccessfully otherwise.</returns>
        public Task<bool> Replace(string key, byte[] buffer, int offset, int count, MemcachedStorageOptions options = null)
        {
            Util.ValidateKey(key);
            Util.ValidateBuffer(buffer, offset, count);
            return ExecuteStoreCommand<ReplaceCommand, bool>(key, buffer, offset, count, options);
        }

        /// <summary>
        /// Appends the specified <paramref name="value"/> to an object in Memcached if the object exists for the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the item to append to. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <param name="value">A <c>byte</c>[] containing the data to be stored in Memcached.</param>
        /// <param name="options">Optional options to pass to Memcached.</param>
        /// <returns>A task that completes with <c>true</c> if the item existed and was appended, <c>false</c> if no item existed for that key, or completes unsuccessfully otherwise.</returns>
        public Task<bool> Append(string key, byte[] value, MemcachedStorageOptions options = null)
        {
            Util.ValidateKey(key);
            if (value == null) throw new ArgumentNullException("value");
            return ExecuteStoreCommand<AppendCommand, bool>(key, value, 0, value.Length, options);
        }

        /// <summary>
        /// Appends the specified data to an object in Memcached if the object exists for the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the item to append to. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <param name="buffer">A <c>byte</c>[] containing the data to be stored in Memcached.</param>
        /// <param name="offset">The point in <paramref name="buffer"/> at which the data to store begins.</param>
        /// <param name="count">The number of bytes in <paramref name="buffer"/> to store.</param>
        /// <param name="options">Optional options to pass to Memcached.</param>
        /// <returns>A task that completes with <c>true</c> if the item existed and was appended, <c>false</c> if no item existed for that key, or completes unsuccessfully otherwise.</returns>
        public Task<bool> Append(string key, byte[] buffer, int offset, int count, MemcachedStorageOptions options = null)
        {
            Util.ValidateKey(key);
            Util.ValidateBuffer(buffer, offset, count);
            return ExecuteStoreCommand<AppendCommand, bool>(key, buffer, offset, count, options);
        }

        /// <summary>
        /// Prepends the specified <paramref name="value"/> to an object in Memcached if the object exists for the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the item to prepend to. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <param name="value">A <c>byte</c>[] containing the data to be stored in Memcached.</param>
        /// <param name="options">Optional options to pass to Memcached.</param>
        /// <returns>A task that completes with <c>true</c> if the item existed and was prepended, <c>false</c> if no item existed for that key, or completes unsuccessfully otherwise.</returns>
        public Task<bool> Prepend(string key, byte[] value, MemcachedStorageOptions options = null)
        {
            Util.ValidateKey(key);
            if (value == null) throw new ArgumentNullException("value");
            return ExecuteStoreCommand<PrependCommand, bool>(key, value, 0, value.Length, options);
        }

        /// <summary>
        /// Prepends the specified data to an object in Memcached if the object exists for the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the item to prepend to. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <param name="buffer">A <c>byte</c>[] containing the data to be stored in Memcached.</param>
        /// <param name="offset">The point in <paramref name="buffer"/> at which the data to store begins.</param>
        /// <param name="count">The number of bytes in <paramref name="buffer"/> to store.</param>
        /// <param name="options">Optional options to pass to Memcached.</param>
        /// <returns>A task that completes with <c>true</c> if the item existed and was prepended, <c>false</c> if no item existed for that key, or completes unsuccessfully otherwise.</returns>
        public Task<bool> Prepend(string key, byte[] buffer, int offset, int count, MemcachedStorageOptions options = null)
        {
            Util.ValidateKey(key);
            Util.ValidateBuffer(buffer, offset, count);
            return ExecuteStoreCommand<PrependCommand, bool>(key, buffer, offset, count, options);
        }

        private Task<TResult> ExecuteStoreCommand<TCommand, TResult>(string key, byte[] buffer, int offset, int count, MemcachedStorageOptions options, long? casUnique = null)
            where TCommand : StorageCommand<TResult>, new()
        {
            return Execute(new TCommand
            {
                Key = key,
                Data = new ArraySegment<byte>(buffer, offset, count),
                Options = options,
                CasUnique = casUnique
            });
        }

        /// <summary>
        /// Increments a counter value in Memcached by the specified <paramref name="value"/> or returns <c>null</c> if the counter value doesn't already exist.
        /// </summary>
        /// <param name="key">The key of the item to increment. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <param name="value">The amount to increment by.</param>
        /// <returns>A task that completes with the value of the counter after the increment, or <c>null</c> if no value exists with that key, or a task that completes unsuccessfully otherwise.</returns>
        public Task<ulong?> Increment(string key, ulong value)
        {
            Util.ValidateKey(key);
            return Execute(new IncrementCommand { Key = key, Value = value });
        }

        /// <summary>
        /// Decrements a counter value in Memcached by the specified <paramref name="value"/> or returns <c>null</c> if the counter value doesn't already exist.
        /// </summary>
        /// <param name="key">The key of the item to decrement. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <param name="value">The amount to decrement by.</param>
        /// <returns>A task that completes with the value of the counter after the decrement, or <c>null</c> if no value exists with that key, or a task that completes unsuccessfully otherwise.</returns>
        public Task<ulong?> Decrement(string key, ulong value)
        {
            Util.ValidateKey(key);
            return Execute(new DecrementCommand { Key = key, Value = value });
        }

        /// <summary>
        /// Stores the specified <paramref name="value"/> in Memcached if the object exists and the specified <paramref name="casUnique"/> flag matches the value in Memcached.
        /// </summary>
        /// <param name="key">The key of the item to store. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <param name="casUnique">The cas unique value of the object previously retrieved from Memcached via <see cref="Gets"/>. See <see cref="MemcachedItem.CasUnique"/>.</param>
        /// <param name="value">A <c>byte</c>[] containing the data to be stored in Memcached if the compare succeeds.</param>
        /// <param name="options">Optional options to pass to Memcached.</param>
        /// <returns>
        ///     <para>A task that completes successfully with a <see cref="CasResult"/> or faults in the event of a failure.</para>
        /// </returns>
        public Task<CasResult> Cas(string key, long casUnique, byte[] value, MemcachedStorageOptions options = null)
        {
            Util.ValidateKey(key);
            if (value == null) throw new ArgumentNullException("value");
            return ExecuteStoreCommand<CasCommand, CasResult>(key, value, 0, value.Length, options, casUnique);
        }

        /// <summary>
        /// Stores the specified data in Memcached if the object exists and the specified <paramref name="casUnique"/> flag matches the value in Memcached.
        /// </summary>
        /// <param name="key">The key of the item to store. Must be between 1 and 250 characters and may not contain whitespace or control characters.</param>
        /// <param name="casUnique">The cas unique value of the object previously retrieved from Memcached via <see cref="Gets"/>. See <see cref="MemcachedItem.CasUnique"/>.</param>
        /// <param name="buffer">A <c>byte</c>[] containing the data to be stored in Memcached if the compare succeeds.</param>
        /// <param name="offset">The point in <paramref name="buffer"/> at which the data to store begins.</param>
        /// <param name="count">The number of bytes in <paramref name="buffer"/> to store.</param>
        /// <param name="options">Optional options to pass to Memcached.</param>
        /// <returns>
        ///     <para>A task that completes successfully with a <see cref="CasResult"/> or faults in the event of a failure.</para>
        /// </returns>
        public Task<CasResult> Cas(string key, long casUnique, byte[] buffer, int offset, int count, MemcachedStorageOptions options = null)
        {
            Util.ValidateKey(key);
            Util.ValidateBuffer(buffer, offset, count);
            return ExecuteStoreCommand<CasCommand, CasResult>(key, buffer, offset, count, options, casUnique);
        }

        /// <summary>
        /// Deletes a value with the specified <paramref name="key"/> from Memcached.
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
#if NET45
                var addresses = await DnsExtensions.GetHostAddressesAsync(_host);
#else
                var addresses = await Dns.GetHostAddressesAsync(_host);
#endif
                address = addresses.First();
            }
            var endpoint = new IPEndPoint(address, _port);
            var conn = new MemcachedConnection(endpoint, _receiveTimeout);
            await conn.Open(_connectTimeout);
            return conn;
        }

        private static bool ValidateConnection(MemcachedConnection connection)
        {
            return connection.State == MemcachedConnectionState.Open;
        }

        /// <summary>
        /// Closes all connections to Memcached and cleans up any other resources that may be in use.
        /// </summary>
        public void Dispose()
        {
            _pool.Dispose();
        }
    }
}
