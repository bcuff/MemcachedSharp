using System;

namespace MemcachedSharp.Commands
{
    internal class DecrementCommand : ArithmeticalCommand
    {
        public override string Verb
        {
            get { return "decr"; }
        }
    }
}
