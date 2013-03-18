using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemcachedSharp.Commands;

namespace MemcachedSharp
{
    internal static class Util
    {
        public static void ValidateKey(string key)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (key.Length > 250) throw new ArgumentException("key must be no more than 250 characters in length", "key");
            for (int i = 0; i < key.Length; ++i)
            {
                if (char.IsWhiteSpace(key[i])) throw new ArgumentException("key may not contain whitespace at position=" + i + " key=" + key, "key");
                if (char.IsControl(key[i])) throw new ArgumentException("key may not contain control characters at position=" + i + " key=" + key, "key");
            }
        }

        public static void ValidateBuffer(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (offset < 0 || offset >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset", "offset must be between 0 and buffer.Length inclusive");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "count must be at least 0");
            }
            if ((offset + count) > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count", "offset + count cannot be greater than buffer.Length");
            }
        }

        public static MemcachedException CreateUnexpectedResponseLine(string responseLine)
        {
            return new MemcachedException("Unexpected response line - " + responseLine);
        }

        public static MemcachedException CreateUnexpectedStorageResponse(StorageCommandResult expected, StorageCommandResult actual)
        {
            return new MemcachedException(string.Format("Unexpceted response from set; expected={0} actual={1}", expected, actual));
        }
    }
}
