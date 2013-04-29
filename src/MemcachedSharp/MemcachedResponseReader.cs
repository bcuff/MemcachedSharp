using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp
{
    internal class MemcachedResponseReader : IResponseReader
    {
        readonly Stream _stream;
        readonly Encoding _encoding;
        readonly TimeSpan _receiveTimeout;
        readonly byte[] _buffer;
        int _pos;
        int _length;

        public MemcachedResponseReader(Stream stream, Encoding encoding, TimeSpan receiveTimeout)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            _stream = stream;
            _encoding = encoding ?? Encoding.UTF8;
            _receiveTimeout = receiveTimeout;
            _buffer = new byte[4 << 10];
        }

        private async Task FillBuffer()
        {
            _length = await Task<int>.Factory.FromAsync(
                (ac, state) => _stream.BeginRead(_buffer, 0, _buffer.Length, ac, state),
                _stream.EndRead,
                null,
                TaskCreationOptions.None)
                .TimeoutAfter(_receiveTimeout);
            if (_length == 0)
            {
                throw new MemcachedException("Unexpected end of stream");
            }
            _pos = 0;
        }

        public async Task<ResponseLine> ReadLine(bool validate = true)
        {
            var ms = new MemoryStream();
            while (true)
            {
                if (_pos == _length) await FillBuffer();
                for (; _pos < _length; ++_pos)
                {
                    if (_buffer[_pos] == '\r') continue;
                    if (_buffer[_pos] == '\n')
                    {
                        ++_pos;
                        ms.Position = 0;
                        var line = _encoding.GetString(ms.ToArray());
                        var result = new ResponseLine(line);
                        if (validate) result.Validate();
                        return result;
                    }
                    ms.WriteByte(_buffer[_pos]);
                }
            }
        }

        public async Task<MemcachedItem> ReadItem()
        {
            var line = await ReadLine();

            if (line.Parts[0] == "END") return null; // end of items
            if (line.Parts[0] != "VALUE") throw new MemcachedException("Invalid response line - " + line);
            if (line.Parts.Length < 4) throw new MemcachedException("Invalid response line - " + line);

            var responseKey = line.Parts[1];
            uint flags;
            int bytes;
            long? casUnique = null;

            if (!uint.TryParse(line.Parts[2], out flags)) throw new MemcachedException("Invalid response line - " + line);
            if (!int.TryParse(line.Parts[3], out bytes) || bytes < 0) throw new MemcachedException("Invalid response line - " + line);
            if (line.Parts.Length > 4)
            {
                long temp;
                if (!long.TryParse(line.Parts[4], out temp)) throw new MemcachedException("Invalid response line - " + line);
                casUnique = temp;
            }

            // read the body of the item
            var body = new MemoryStream(bytes);
            while (bytes > 0)
            {
                if (_pos == _length) await FillBuffer();
                var count = Math.Min(bytes, _length - _pos);
                body.Write(_buffer, _pos, count);
                _pos += count;
                bytes -= count;
            }
            body.Position = 0;

            // read the endline
            while (true)
            {
                if (_pos == _length) await FillBuffer();
                if (_buffer[_pos] == '\r') { ++_pos; continue; }
                if (_buffer[_pos] == '\n') { ++_pos; break; }
                throw new MemcachedException("Unexpected character encountered - (byte)" + _buffer[_pos]);
            }

            return new MemcachedItem(responseKey, flags, body.Length, casUnique, body);
        }
    }
}
