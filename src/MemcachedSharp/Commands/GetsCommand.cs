using System;

namespace MemcachedSharp.Commands
{
    internal class GetsCommand : RetrievalCommand
    {
        public override string Verb
        {
            get { return "gets"; }
        }
    }
}
