using System;
using System.Linq;

namespace MemcachedSharp.Commands
{
    internal class ReplaceCommand : StorageCommand
    {
        public override string Verb
        {
            get { return "replace"; }
        }

        protected override void ValidateResponse(StorageCommandResult result, string responseLine)
        {
            if (result != StorageCommandResult.Stored && result != StorageCommandResult.NotStored)
            {
                throw CreateUnexpectedResponse(responseLine);
            }
        }
    }
}
