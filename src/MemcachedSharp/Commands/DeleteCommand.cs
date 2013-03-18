using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp.Commands
{
    class DeleteCommand : SingleKeyCommand<bool>
    {
        public override Task SendRequest(ISocket socket)
        {
            var line = string.Format("delete " + Key + "\r\n").ToUtf8();
            return socket.SendAsync(line);
        }

        public override async Task<bool> ReadResponse(IResponseReader reader)
        {
            var responseLine = await reader.ReadLine();
            if (responseLine.Parts[0] == "DELETED") return true;
            if (responseLine.Parts[0] == "NOT_FOUND") return false;
            throw Util.CreateUnexpectedResponseLine(responseLine.Line);
        }
    }
}
