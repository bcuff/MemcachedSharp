using System;
using System.Linq;

namespace MemcachedSharp.Commands
{
    internal class ReplaceCommand : StorageCommand<bool>
    {
        public override string Verb
        {
            get { return "replace"; }
        }

        protected override bool TryConvertResult(StorageCommandResult storageResult, out bool actualResult)
        {
            actualResult = storageResult == StorageCommandResult.Stored;
            return actualResult || storageResult == StorageCommandResult.NotStored;
        }
    }
}
