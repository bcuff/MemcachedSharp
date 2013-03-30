using System;

namespace MemcachedSharp.Commands
{
    internal class AppendCommand : StorageCommand<bool>
    {
        public override string Verb
        {
            get { return "append"; }
        }

        protected override bool TryConvertResult(StorageCommandResult storageResult, out bool actualResult)
        {
            actualResult = storageResult == StorageCommandResult.Stored;
            return actualResult || storageResult == StorageCommandResult.NotStored;
        }
    }
}
