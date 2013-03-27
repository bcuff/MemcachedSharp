using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MemcachedSharp.Test.Pooling
{
    [TestClass]
    public class PipelinedPoolTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestContructorValidatesNullCreateFactory()
        {
            new PipelinedPool<int>(null);
        }

        [TestMethod]
        public async Task TestSimpleReuse()
        {
            var i = 0;
            var pool = new PipelinedPool<int>(() => TaskPort.FromResult(++i));
            using (var item = await pool.Borrow())
            {
                Assert.AreEqual(1, item.Item);
            }

            using (var item = await pool.Borrow())
            {
                Assert.AreEqual(1, item.Item);
            }
        }

        [TestMethod]
        public async Task TestMaxItemCount()
        {
            var nextItem = 0;
            var pool = new PipelinedPool<int>(() => TaskPort.FromResult(++nextItem), new PipelinedPoolOptions
            {
                MaxRequestsPerItem = 10,
                TargetItemCount = 2,
            });

            var items = new List<IPooledItem<int>>();
            for (int i = 0; i < 20; ++i)
            {
                var task = pool.Borrow();
                Assert.AreEqual(true, task.IsCompleted);
                var item = await task;
                items.Add(item);
            }
            Assert.AreEqual(20, items.Count);
            Assert.AreEqual(10, items.Count(v => v.Item == 1));
            Assert.AreEqual(10, items.Count(v => v.Item == 2));

            var waiter = pool.Borrow();
            Assert.AreEqual(false, waiter.IsCompleted);

            items.First().Dispose();
            Assert.AreEqual(true, waiter.IsCompleted);
            foreach (var item in items.Skip(1))
            {
                item.Dispose();
            }
            waiter.Result.Dispose();
        }
    }
}
