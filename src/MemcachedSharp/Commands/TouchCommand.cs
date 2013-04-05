using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp.Commands
{
    internal class TouchCommand : SingleKeyCommand<bool>
    {
        public override string Verb
        {
            get { return "touch"; }
        }

        public TimeSpan ExpirationTime { get; set; }

        public override Task SendRequest(ISocket socket)
        {
            var seconds = (long)ExpirationTime.TotalSeconds;
            var line = string.Format("{0} {1} {2}\r\n", Verb, Key, seconds).ToUtf8();
            return socket.SendAsync(line);
        }

        public override async Task<bool> ReadResponse(IResponseReader reader)
        {
            var line = await reader.ReadLine();
            var result = line.Parts[0];
            if (result == "TOUCHED") return true;
            if (result == "NOT_FOUND") return false;
            throw Util.CreateUnexpectedResponseLine(this, line.Line);
        }
    }
}
