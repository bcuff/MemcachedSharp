using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            var pool = new PipelinedPool<int>(() => TaskPort.FromResult(++nextItem), null, new PipelinedPoolOptions
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

        [TestMethod]
        public async Task TestItemValidator()
        {
            var items = new List<MockItem>();
            var pool = new PipelinedPool<MockItem>(() =>
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
            static int _nextId;
            int _id;

            public MockItem()
            {
                _id = Interlocked.Increment(ref _nextId);
                IsValid = true;
            }

            public int Id
            {
                get { return _id; }
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
