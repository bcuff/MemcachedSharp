﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp.Commands
{
    internal class PluralGetCommand : PluralRetrievalCommand
    {
        public override string Verb
        {
            get { return "get"; }
        }
    }
}