using System;

namespace MemcachedSharp.Commands
{
    internal class SetCommand : StorageCommand
    {
        public override string Verb
        {
            get { return "set"; }
        }

        protected override void ValidateResponse(StorageCommandResult result, string responseLine)
        {
            if (result != StorageCommandResult.Stored) throw new MemcachedException("Unexpected response from set command; responseLine=" + responseLine);
        }
    }
}
