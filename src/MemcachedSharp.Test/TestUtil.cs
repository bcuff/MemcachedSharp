using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MemcachedSharp.Test
{
    internal static class TestUtil
    {
        public static string ReadLine(this Stream stream, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            var result = new StringBuilder();
            var decoder = encoding.GetDecoder();
            while (true)
            {
                var val = ReadMinimum(stream, decoder);
                if (val == null) throw new EndOfStreamException();
                if (val == "\n") return result.ToString();
                if (val != "\r")
                {
                    result.Append(val);
                }
            }
        }

        public static string ReadMinimum(this Stream stream, Decoder decoder)
        {
            var buffer = new byte[16];
            for (int i = 0; i < buffer.Length; ++i)
            {
                var read = stream.Read(buffer, i, 1);
                if (read == 0)
                {
                    if (i != 0) throw new Exception("invalid encoded text?");
                    return null;
                }
                var charCount = decoder.GetCharCount(buffer, 0, i + 1);
                if (charCount != 0)
                {
                    var chars = new char[charCount];
                    decoder.GetChars(buffer, 0, i + 1, chars, 0);
                    return new string(chars);
                }
            }
            throw new Exception("impossible?");
        }
    }
}
