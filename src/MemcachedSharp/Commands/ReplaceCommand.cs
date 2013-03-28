using System;
using System.Linq;

namespace MemcachedSharp.Commands
{
    internal class ReplaceCommand : StorageCommand
    {
        public override string Verb
        {
            get { return "replace"; }
        }
    }
}
