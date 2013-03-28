using System;

namespace MemcachedSharp.Commands
{
    internal class AddCommand : StorageCommand
    {
        public override string Verb
        {
            get { return "add"; }
        }
    }
}
