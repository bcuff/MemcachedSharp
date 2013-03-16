using System;
using System.Linq;

namespace MemcachedSharp
{
    static class DateTimeExtensions
    {
        public static uint ToUnixTimestamp(this DateTime dateTime)
        {
            return (uint)(dateTime - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
        }
    }
}
