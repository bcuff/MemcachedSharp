﻿using System;

namespace MemcachedSharp.Commands
{
    internal class GetCommand : RetrievalCommand
    {
        public override string Verb
        {
            get { return "get"; }
        }
    }
}
