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

        protected override bool IsResultValid(StorageCommandResult result)
        {
            return result == StorageCommandResult.Stored || result == StorageCommandResult.NotStored;
        }
    }
}
