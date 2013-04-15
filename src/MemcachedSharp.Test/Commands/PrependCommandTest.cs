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
    public class PrependCommandTest
    {
        [TestMethod]
        public async Task TestSendRequest()
        {
            await StorageCommandValidator.TestSendBehavior<PrependCommand, bool>("prepend");
        }
        [TestMethod]
        public async Task ReadResponse()
        {
            var command = new PrependCommand();
            await StorageCommandValidator.AssertReadResponse(command, StorageCommandResult.Stored, true);
            await StorageCommandValidator.AssertReadResponse(command, StorageCommandResult.NotStored, false);
            await StorageCommandValidator.AssertReadResponseFailure<PrependCommand, bool>(command, StorageCommandResult.NotFound);
            await StorageCommandValidator.AssertReadResponseFailure<PrependCommand, bool>(command, StorageCommandResult.Exists);
        }
    }
}
