using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MemcachedSharp.Test.Pooling
{
    [TestClass]
    public class PoolTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestContructorValidatesNullCreateFactory()
        {
            new Pool<int>(null);
        }

        [TestMethod]
        public async Task TestItemValidator()
        {
            var items = new List<MockItem>();
            var pool = new Pool<MockItem>(() =>
            {
                var result = new MockItem();
                items.Add(result);
                return TaskPort.FromResult(result);
            }, item => item.IsValid);

            using (var item = await pool.Borrow())
            {
                item.Item.IsValid = false;
            }

            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, items.First().DisposeCount);

            using (var item = await pool.Borrow())
            {
                item.Item.IsValid = true;
            }

            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0].DisposeCount);
            Assert.AreEqual(0, items[1].DisposeCount);

            using (var item = await pool.Borrow())
            {
                Assert.AreSame(items[1], item.Item);
            }

            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0].DisposeCount);
            Assert.AreEqual(0, items[1].DisposeCount);
        }

        private class MockItem : IDisposable
        {
            public MockItem()
            {
                IsValid = true;
            }

            public bool IsValid { get; set; }

            public int DisposeCount { get; set; }

            public void Dispose()
            {
                DisposeCount++;
            }
        }
     }
}
