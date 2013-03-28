using System;

namespace MemcachedSharp.Commands
{
    internal class AppendCommand : StorageCommand
    {
        public override string Verb
        {
            get { return "append"; }
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
