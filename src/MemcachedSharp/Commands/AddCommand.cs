using System;

namespace MemcachedSharp.Commands
{
    internal class AddCommand : StorageCommand
    {
        public override string Verb
        {
            get { return "add"; }
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
