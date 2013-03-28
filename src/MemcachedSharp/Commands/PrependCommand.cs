using System;

namespace MemcachedSharp.Commands
{
    internal class PrependCommand : StorageCommand
    {
        public override string Verb
        {
            get { return "prepend"; }
        }

        protected override void ValidateResponse(StorageCommandResult result, string responseLine)
        {
            if (result != StorageCommandResult.Stored || result != StorageCommandResult.NotStored)
            {
                throw CreateUnexpectedResponse(responseLine);
            }
        }
    }
}
