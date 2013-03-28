using System;

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
