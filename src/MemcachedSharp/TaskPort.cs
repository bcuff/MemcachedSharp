using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MemcachedSharp
{
    internal static class TaskPort
    {
        public static Task Delay(TimeSpan dueTime, CancellationToken cancellationToken)
        {
#if NET40
            return TaskEx.Delay(dueTime, cancellationToken);
#else
            return Task.Delay(dueTime, cancellationToken);
#endif
        }

        public static Task<Task> WhenAny(params Task[] tasks)
        {
#if NET40
            return TaskEx.WhenAny(tasks);
#else
            return Task.WhenAny(tasks);
#endif
        }

        public static Task<T> FromResult<T>(T value)
        {
#if NET40
            return TaskEx.FromResult(value);
#else
            return Task.FromResult(value);
#endif
        }
    }
}
