using System;

namespace MemcachedSharp.Commands
{
    internal class CasCommand : StorageCommand
    {
        public override string Verb
        {
            get { return "cas"; }
        }
    }
}
