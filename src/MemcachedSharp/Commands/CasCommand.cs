using System;

namespace MemcachedSharp.Commands
{
    internal class CasCommand : StorageCommand<CasResult>
    {
        public override string Verb
        {
            get { return "cas"; }
        }

        protected override bool TryConvertResult(StorageCommandResult storageResult, out CasResult actualResult)
        {
            switch(storageResult)
            {
                case StorageCommandResult.Stored: actualResult = CasResult.Stored; return true;
                case StorageCommandResult.Exists: actualResult = CasResult.Exists; return true;
                case StorageCommandResult.NotFound: actualResult = CasResult.NotFound; return true;
                default: actualResult = default(CasResult); return false;
            }
        }
    }
}
