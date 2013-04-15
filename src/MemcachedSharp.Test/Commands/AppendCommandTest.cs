using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemcachedSharp.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MemcachedSharp.Test.Commands
{
    [TestClass]
    public class AppendCommandTest
    {
        [TestMethod]
        public async Task TestSendRequest()
        {
            await StorageCommandValidator.TestSendBehavior<AppendCommand, bool>("append");
        }
        [TestMethod]
        public async Task ReadResponse()
        {
            var command = new AppendCommand();
            await StorageCommandValidator.AssertReadResponse(command, StorageCommandResult.Stored, true);
            await StorageCommandValidator.AssertReadResponse(command, StorageCommandResult.NotStored, false);
            await StorageCommandValidator.AssertReadResponseFailure<AppendCommand, bool>(command, StorageCommandResult.NotFound);
            await StorageCommandValidator.AssertReadResponseFailure<AppendCommand, bool>(command, StorageCommandResult.Exists);
        }
    }
}
