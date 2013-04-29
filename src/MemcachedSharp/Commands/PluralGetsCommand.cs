using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp.Commands
{
    internal class PluralGetsCommand : PluralRetrievalCommand
    {
        public override string Verb
        {
            get { return "gets"; }
        }
    }
}
