using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemcachedSharp
{
    internal static class StringExtensions
    {
        private static readonly Encoding _utf8NoBom = new UTF8Encoding(false, true);

        public static byte[] ToUtf8(this string value)
        {
            if (value == null) throw new ArgumentNullException("value");

            return _utf8NoBom.GetBytes(value);
        }
    }
}
