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
    public class AddCommandTest
    {
        [TestMethod]
        public async Task TestSendRequest()
        {
            await StorageCommandValidator.TestSendBehavior<AddCommand, bool>("add");
        }

        [TestMethod]
        public async Task ReadResponse()
        {
            var command = new AddCommand();
            await StorageCommandValidator.AssertReadResponse(command, StorageCommandResult.Stored, true);
            await StorageCommandValidator.AssertReadResponse(command, StorageCommandResult.NotStored, false);
            await StorageCommandValidator.AssertReadResponseFailure<AddCommand, bool>(command, StorageCommandResult.NotFound);
            await StorageCommandValidator.AssertReadResponseFailure<AddCommand, bool>(command, StorageCommandResult.Exists);
        }
    }
}
