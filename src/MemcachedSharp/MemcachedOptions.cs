using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemcachedSharp
{
    public class MemcachedOptions
    {
        public MemcachedOptions()
        {
            ConnectTimeout = TimeSpan.FromSeconds(2);
            ReceiveTimeout = TimeSpan.FromSeconds(2);
            MaxConnections = 2;
            MaxConcurrentRequestPerConnection = 15;
            EnablePipelining = true;
        }

        public TimeSpan ConnectTimeout { get; set; }
        public TimeSpan ReceiveTimeout { get; set; }
        public bool EnablePipelining { get; set; }
        public int MaxConcurrentRequestPerConnection { get; set; }
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
