using System;

namespace MemcachedSharp.Commands
{
    internal class IncrementCommand : ArithmeticalCommand
    {
        public override string Verb
        {
            get { return "incr"; }
        }
    }
}
