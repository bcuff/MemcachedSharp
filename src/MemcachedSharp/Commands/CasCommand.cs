using System;

namespace MemcachedSharp.Commands
{
    internal class CasCommand : StorageCommand
    {
        public override string Verb
        {
            get { return "cas"; }
        }

        protected override void ValidateResponse(StorageCommandResult result, string responseLine)
        {
            switch (result)
            {
                case StorageCommandResult.Stored:
                case StorageCommandResult.Exists:
                case StorageCommandResult.NotFound:
                    break;
                default:
                    throw CreateUnexpectedResponse(responseLine);
            }
        }
    }
}
