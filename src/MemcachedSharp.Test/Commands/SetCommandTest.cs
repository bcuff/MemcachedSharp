using System;
using System.Linq;
using System.Threading.Tasks;
using MemcachedSharp.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MemcachedSharp.Test.Commands
{
    [TestClass]
    public class SetCommandTest
    {
        [TestMethod]
        public async Task TestSendRequest()
        {
            await StorageCommandValidator.TestSendBehavior<SetCommand, bool>("set");
        }
        [TestMethod]
        public async Task ReadResponse()
        {
            var command = new SetCommand();
            await StorageCommandValidator.AssertReadResponse(command, StorageCommandResult.Stored, true);
            await StorageCommandValidator.AssertReadResponseFailure<SetCommand, bool>(command, StorageCommandResult.NotStored);
            await StorageCommandValidator.AssertReadResponseFailure<SetCommand, bool>(command, StorageCommandResult.NotFound);
            await StorageCommandValidator.AssertReadResponseFailure<SetCommand, bool>(command, StorageCommandResult.Exists);
        }
    }
}
