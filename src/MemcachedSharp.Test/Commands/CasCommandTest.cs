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
    public class CasCommandTest
    {
        [TestMethod]
        public async Task TestSendRequest()
        {
            await StorageCommandValidator.TestSendBehavior<CasCommand, CasResult>("cas");
        }

        [TestMethod]
        public async Task ReadResponse()
        {
            var command = new CasCommand();
            await StorageCommandValidator.AssertReadResponse(command, StorageCommandResult.Stored, CasResult.Stored);
            await StorageCommandValidator.AssertReadResponseFailure<CasCommand, CasResult>(command, StorageCommandResult.NotStored);
            await StorageCommandValidator.AssertReadResponse(command, StorageCommandResult.Exists, CasResult.Exists);
            await StorageCommandValidator.AssertReadResponse(command, StorageCommandResult.NotFound, CasResult.NotFound);
        }
    }
}
