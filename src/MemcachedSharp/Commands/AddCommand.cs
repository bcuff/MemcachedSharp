using System;

namespace MemcachedSharp.Commands
{
    internal class AddCommand : StorageCommand<bool>
    {
        public override string Verb
        {
            get { return "add"; }
        }

        protected override bool TryConvertResult(StorageCommandResult storageResult, out bool actualResult)
        {
            actualResult = storageResult == StorageCommandResult.Stored;
            return actualResult || storageResult == StorageCommandResult.NotStored;
        }
    }
}
