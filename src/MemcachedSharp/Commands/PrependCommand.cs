using System;

namespace MemcachedSharp.Commands
{
    internal class PrependCommand : StorageCommand
    {
        public override string Verb
        {
            get { return "prepend"; }
        }

        protected override bool IsResultValid(StorageCommandResult result)
        {
            return result == StorageCommandResult.Stored || result == StorageCommandResult.NotStored;
        }
    }
}
