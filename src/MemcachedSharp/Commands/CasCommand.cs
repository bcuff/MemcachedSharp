using System;

namespace MemcachedSharp.Commands
{
    internal class CasCommand : StorageCommand
    {
        public override string Verb
        {
            get { return "cas"; }
        }

        protected override bool IsResultValid(StorageCommandResult result)
        {
            return result == StorageCommandResult.Stored
                || result == StorageCommandResult.Exists
                || result == StorageCommandResult.NotFound;
        }
    }
}
