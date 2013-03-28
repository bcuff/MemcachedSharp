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
    public class SetCommandTest
    {
        [TestMethod]
        public async Task TestSendRequest()
        {
            foreach (var command in StorageCommandValidator.GenerateTestCommands<SetCommand>())
            {
                await StorageCommandValidator.TestSendBehavior(command);
            }
        }
    }
}
