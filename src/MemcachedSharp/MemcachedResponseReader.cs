using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp
{
    internal class MemcachedResponseReader
    {
        readonly Stream _stream;
        readonly Encoding _encoding;
        readonly byte[] _buffer;
        int _pos;
        int _length;

        public MemcachedResponseReader(Stream stream, Encoding encoding)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            _stream = stream;
            _encoding = encoding ?? Encoding.UTF8;
            _buffer = new byte[4 << 10];
        }

        private async Task<bool> EnsureBuffer()
        {
            if (_pos == _length)
            {
                _length = await Task<int>.Factory.FromAsync(
                    (ac, state) => _stream.BeginRead(_buffer, 0, _buffer.Length, ac, state),
                    _stream.EndRead,
                    null,
                    TaskCreationOptions.None);
                if (_length == 0)
                {
                    return false;
                }
                _pos = 0;
            }
            return true;
        }

        public async Task<string> ReadLine()
        {
            var ms = new MemoryStream();
            while (await EnsureBuffer())
            {
                for (; _pos < _length; ++_pos)
                {
                    if (_buffer[_pos] == '\r') continue;
                    if (_buffer[_pos] == '\n')
                    {
                        ++_pos;
                        ms.Position = 0;
                        return _encoding.GetString(ms.ToArray());
                    }
                    ms.WriteByte(_buffer[_pos]);
                }
            }

            throw new EndOfStreamException("Unexpected end of stream");
        }

        public async Task<MemoryStream> ReadBody(int length)
        {
            var ms = new MemoryStream(length);
            while (length > 0 && await EnsureBuffer())
            {
                var count = Math.Min(length, _length - _pos);
                await ms.WriteAsync(_buffer, _pos, count);
                _pos += count;
                length -= count;
            }
            ms.Position = 0;
            return ms;
        }
    }
}
