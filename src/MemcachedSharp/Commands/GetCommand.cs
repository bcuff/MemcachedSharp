using System;
using System.Threading.Tasks;

namespace MemcachedSharp.Commands
{
    class GetCommand : SingleKeyCommand<MemcachedItem>
    {
        public override Task SendRequest(ISocket socket)
        {
            var line = string.Format("get " + Key + "\r\n").ToUtf8();
            return socket.SendAsync(line);
        }

        public override async Task<MemcachedItem> ReadResponse(IResponseReader reader)
        {
            var result = await reader.ReadItem();
            if (result != null)
            {
                var nextItem = await reader.ReadItem();
                if (nextItem != null)
                {
                    throw new MemcachedException("Memcached returned more items than expected.");
                }
            }
            return result;
        }
    }
}
