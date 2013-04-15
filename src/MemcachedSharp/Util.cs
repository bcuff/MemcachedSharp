using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemcachedSharp.Commands;

namespace MemcachedSharp
{
    internal static class Util
    {
        static readonly Dictionary<string, StorageCommandResult> _storageResults = new Dictionary<string, StorageCommandResult>(4)
        {
            { "STORED", StorageCommandResult.Stored },
            { "NOT_STORED", StorageCommandResult.NotStored },
            { "EXISTS", StorageCommandResult.Exists },
            { "NOT_FOUND", StorageCommandResult.NotFound },
        };

        static readonly Dictionary<StorageCommandResult, string> _storageResultVerbs;

        static Util()
        {
            _storageResultVerbs = new Dictionary<StorageCommandResult, string>(_storageResults.Count);
            foreach (var pair in _storageResults)
            {
                _storageResultVerbs.Add(pair.Value, pair.Key);
            }
        }

        public static bool TryParseStorageCommandResult(string verb, out StorageCommandResult result)
        {
            return _storageResults.TryGetValue(verb, out result);
        }

        public static string ToVerb(this StorageCommandResult value)
        {
            string result;
            if (!_storageResultVerbs.TryGetValue(value, out result))
            {
                throw new ArgumentException("Invalid StorageCommandResult - " + value, "value");
            }
            return result;
        }

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

        public static MemcachedException CreateUnexpectedResponseLine(ICommand command, string responseLine)
        {
            var message = string.Format("Unexpected response from '{0}' command; responseLine={1}", command.Verb, responseLine);
            return new MemcachedException(message);
        }
    }
}
