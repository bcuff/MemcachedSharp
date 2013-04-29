using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp.Commands
{
    internal abstract class PluralRetrievalCommand : ICommand<bool>
    {
        public IEnumerable<string> Keys { get; set; }
        public event Action<MemcachedItem> OnItem;

        public Task SendRequest(ISocket socket)
        {
            var line = new StringBuilder(Verb);
            foreach (var key in Keys)
            {
                line.Append(' ');
                line.Append(key);
            }
            line.Append("\r\n");
            return socket.SendAsync(line.ToString().ToUtf8());
        }

        public async Task<bool> ReadResponse(IResponseReader reader)
        {
            while(true)
            {
                var item = await reader.ReadItem();
                if (item == null) break;
                if (OnItem != null) OnItem(item);
            }
            return true;
        }

        public abstract string Verb { get; }
    }
}
