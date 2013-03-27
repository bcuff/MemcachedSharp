using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MemcachedSharp
{
    internal static class TaskExtensions
    {
        static readonly TimeSpan _infinite = TimeSpan.FromMilliseconds(Timeout.Infinite);

        public static async Task TimeoutAfter(this Task task, TimeSpan timeout)
        {
            if (task == null) throw new ArgumentNullException("task");

            if (timeout == _infinite || task.IsCompleted)
            {
                task.Wait();
                return;
            }

            var token = new CancellationTokenSource();
            var delay = Task.Delay(timeout, token.Token);
            var result = await Task.WhenAny(new[] { task, delay });
            if (result == delay)
            {
                throw new TimeoutException("Task timed out after " + timeout);
            }
            token.Cancel();
            task.Wait();
        }

        public static async Task<T> TimeoutAfter<T>(this Task<T> task, TimeSpan timeout)
        {
            if (task == null) throw new ArgumentNullException("task");

            if (timeout == _infinite || task.IsCompleted) return await task;

            var token = new CancellationTokenSource();
            var delay = Task.Delay(timeout, token.Token);
            var result = await Task.WhenAny(new[] { task, delay });
            if (result == delay)
            {
                throw new TimeoutException("Task timed out after " + timeout);
            }
            token.Cancel();
            return await task;
        }
    }
}
