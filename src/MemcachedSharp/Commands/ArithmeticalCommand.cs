using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp.Commands
{
    internal abstract class ArithmeticalCommand : SingleKeyCommand<ulong?>
    {
        public ulong Value { get; set; }

        public override Task SendRequest(ISocket socket)
        {
            var line = string.Format("{0} {1} {2}\r\n", Verb, Key, Value).ToUtf8();
            return socket.SendAsync(line);
        }

        public override async Task<ulong?> ReadResponse(IResponseReader reader)
        {
            var line = await reader.ReadLine();
            if (line.Parts[0] == "NOT_FOUND")
            {
                return null;
            }
            ulong result;
            if (ulong.TryParse(line.Parts[0], out result))
            {
                return result;
            }
            throw Util.CreateUnexpectedResponseLine(this, line.Line);
        }
    }
}
