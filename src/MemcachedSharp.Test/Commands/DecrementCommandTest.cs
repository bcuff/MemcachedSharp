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
    public class DecrementCommandTest
    {
        [TestMethod]
        public void TestVerb()
        {
            var command = new DecrementCommand();
            Assert.AreEqual("decr", command.Verb);
        }

        [TestMethod]
        public async Task TestSendBehavior()
        {
            await ArithmeticalCommandValidator.TestSendBehavior<DecrementCommand>("decr");
        }
    }
}
