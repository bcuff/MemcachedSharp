using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MemcachedSharp.Test
{
    [TestClass]
    public class TaskExtensionsTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestTimeoutAfterArgNullValidation()
        {
            await TaskExtensions.TimeoutAfter(null, TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestTimeoutAfterOfTArgNullValidation()
        {
            await TaskExtensions.TimeoutAfter<string>(null, TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task TestTimeoutAfterWithTimeout()
        {
            var source = new TaskCompletionSource<int>();
            var task = (Task)source.Task;
            await task.TimeoutAfter(TimeSpan.FromMilliseconds(100));
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task TestTimeoutAfterOfTWithTimeout()
        {
            var source = new TaskCompletionSource<int>();
            await source.Task.TimeoutAfter(TimeSpan.FromMilliseconds(100));
        }

        [TestMethod]
        public async Task TestTimeoutAfterCompletesNormally()
        {
            var source = new TaskCompletionSource<int>();
            var task = (Task)source.Task;
            task = task.TimeoutAfter(TimeSpan.FromMilliseconds(100));
            source.SetResult(1);
            await task;
        }

        [TestMethod]
        public async Task TestTimeoutAfterOfTCompletesNormally()
        {
            var source = new TaskCompletionSource<int>();
            var task = source.Task.TimeoutAfter(TimeSpan.FromMilliseconds(100));
            source.SetResult(1);
            Assert.AreEqual(1, await task);
        }
    }
}
