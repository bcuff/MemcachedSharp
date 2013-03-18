using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemcachedSharp.Commands
{
    enum StorageCommandResult
    {
        Stored,
        NotStored,
        Exists,
        NotFound,
    }
}
