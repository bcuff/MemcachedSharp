using System;

namespace MemcachedSharp.Commands
{
    internal class AppendCommand : StorageCommand
    {
        public override string Verb
        {
            get { return "append"; }
        }

        protected override bool IsResultValid(StorageCommandResult result)
        {
            return result == StorageCommandResult.Stored || result == StorageCommandResult.NotStored;
        }
    }
}
