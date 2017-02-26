#if NET45
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp
{
    static class DnsExtensions
    {
        public static Task<IPAddress[]> GetHostAddressesAsync(string hostNameOrAddress)
        {
            // todo - add timeout
            return Task<IPAddress[]>.Factory.FromAsync(
                (ac, state) => Dns.BeginGetHostAddresses(hostNameOrAddress, ac, state),
                Dns.EndGetHostAddresses,
                null,
                TaskCreationOptions.None);
        }
    }
}
#endif
