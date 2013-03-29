using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp.Commands
{
    abstract class StorageCommand : SingleKeyCommand<StorageCommandResult>
    {
        static readonly byte[] _endLineBuffer = Encoding.UTF8.GetBytes("\r\n");
        static readonly Dictionary<string, StorageCommandResult> _storageResults = new Dictionary<string, StorageCommandResult>
        {
            { "STORED", StorageCommandResult.Stored },
            { "NOT_STORED", StorageCommandResult.NotStored },
            { "EXISTS", StorageCommandResult.Exists },
            { "NOT_FOUND", StorageCommandResult.NotFound },
        };

        public MemcachedStorageOptions Options { get; set; }
        public ArraySegment<byte> Data { get; set; }

        public sealed override Task SendRequest(ISocket socket)
        {
            uint flags = 0;
            uint expires = 0;
            if (Options != null)
            {
                flags = Options.Flags;
                if (Options.ExpirationTime.HasValue)
                {
                    expires = (uint)Options.ExpirationTime.Value.TotalSeconds;
                }
            }
            var line = string.Format("{0} {1} {2} {3} {4}\r\n", Verb, Key, flags, expires, Data.Count);
            return socket.SendAsync(new[]
            {
                new ArraySegment<byte>(line.ToUtf8()),
                Data,
                new ArraySegment<byte>(_endLineBuffer),
            });
        }

        public override sealed async Task<StorageCommandResult> ReadResponse(IResponseReader reader)
        {
            var line = await reader.ReadLine();
            StorageCommandResult result;
            if (_storageResults.TryGetValue(line.Parts[0], out result) && IsResultValid(result))
            {
                return result;
            }
            throw Util.CreateUnexpectedResponseLine(this, line.Line);
        }

        protected abstract bool IsResultValid(StorageCommandResult result);
    }
}
