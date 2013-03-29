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
    public class IncrementCommandTest
    {
        [TestMethod]
        public void TestVerb()
        {
            var command = new IncrementCommand();
            Assert.AreEqual("incr", command.Verb);
        }

        [TestMethod]
        public async Task TestSendBehavior()
        {
            await ArithmeticalCommandValidator.TestSendBehavior<IncrementCommand>("incr");
        }
    }
}
