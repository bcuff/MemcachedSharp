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
    }
}
