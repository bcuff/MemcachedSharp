using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp.Commands
{
    class SetCommand : StorageCommand<StorageCommandResult>
    {
        static readonly Dictionary<string, StorageCommandResult> _storageResults = new Dictionary<string, StorageCommandResult>
        {
            { "STORED", StorageCommandResult.Stored },
            { "NOT_STORED", StorageCommandResult.NotStored },
            { "EXISTS", StorageCommandResult.Exists },
            { "NOT_FOUND", StorageCommandResult.NotFound },
        };

        public override string Verb
        {
            get { return "set"; }
        }

        public override async Task<StorageCommandResult> ReadResponse(IResponseReader reader)
        {
            var line = await reader.ReadLine();
            StorageCommandResult result;
            if (_storageResults.TryGetValue(line.Parts[0], out result))
            {
                return result;
            }
            throw Util.CreateUnexpectedResponseLine(line.Line);
        }
    }
}
