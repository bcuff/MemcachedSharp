using System;

namespace MemcachedSharp.Commands
{
    internal class SetCommand : StorageCommand<bool>
    {
        public override string Verb
        {
            get { return "set"; }
        }

        protected override bool TryConvertResult(StorageCommandResult storageResult, out bool actualResult)
        {
            if (storageResult == StorageCommandResult.Stored)
            {
                actualResult = true;
                return true;
            }
            actualResult = false;
            return false;
        }
    }
}
