using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemcachedSharp
{
    /// <summary>
    /// Encapsulates options for <see cref="MemcachedClient"/>.
    /// </summary>
    public sealed class MemcachedOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemcachedOptions"/> class.
        /// </summary>
        public MemcachedOptions()
        {
            ConnectTimeout = TimeSpan.FromSeconds(2);
            ReceiveTimeout = TimeSpan.FromSeconds(2);
            MaxConnections = 2;
            MaxConcurrentRequestPerConnection = 15;
            EnablePipelining = true;
        }

        /// <summary>
        /// Gets or sets the amount of time to wait before abandoning an attempt to connect to a Memcached host.
        /// 2 seconds by default.
        /// </summary>
        public TimeSpan ConnectTimeout { get; set; }

        /// <summary>
        /// Gets or sets the amount of time to wait before abandoning an attempt to receive more data from a Memcached host.
        /// 2 seconds by default.
        /// </summary>
        public TimeSpan ReceiveTimeout { get; set; }

        /// <summary>
        /// Gets or sets a value that enables pipelining multiple requests on the same connections. <c>true</c> by default.
        /// </summary>
        public bool EnablePipelining { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of requests that may be sent at one time on the same connection.
        /// Only applicable if <see cref="EnablePipelining"/> is <c>true</c>.
        /// 15 by default.
        /// </summary>
        public int MaxConcurrentRequestPerConnection { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of connections that may be opened at one time for the target <see cref="MemcachedClient"/> instance.
        /// </summary>
        public int MaxConnections { get; set; }

        internal void Validate(string parameterName)
        {
            if (ConnectTimeout <= TimeSpan.Zero && ConnectTimeout != TimeSpan.FromMilliseconds(-1))
            {
                throw new ArgumentException("ConnectTimeout must be a positive value or -1 millisecond for infinite timeouts", parameterName);
            }
            if (EnablePipelining && MaxConcurrentRequestPerConnection < 1)
            {
                throw new ArgumentException("MaxConcurrentRequestPerConnection must be greater than or equal to 1", parameterName);
            }
            if (MaxConnections < 1)
            {
                throw new ArgumentException("MaxConnections must be greater than or equal to 1", parameterName);
            }
        }
    }
}
