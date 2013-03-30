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
    }
}
