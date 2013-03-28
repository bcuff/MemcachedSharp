using System;

namespace MemcachedSharp.Commands
{
    internal class PrependCommand : StorageCommand
    {
        public override string Verb
        {
            get { return "prepend"; }
        }
    }
}
