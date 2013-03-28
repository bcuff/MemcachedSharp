using System;

namespace MemcachedSharp.Commands
{
    internal class AppendCommand : StorageCommand
    {
        public override string Verb
        {
            get { return "append"; }
        }
    }
}
