using System;

namespace MemcachedSharp.Commands
{
    internal class AddCommand : StorageCommand
    {
        public override string Verb
        {
            get { return "add"; }
        }

        protected override bool IsResultValid(StorageCommandResult result)
        {
            return result == StorageCommandResult.Stored || result == StorageCommandResult.NotStored;
        }
    }
}
