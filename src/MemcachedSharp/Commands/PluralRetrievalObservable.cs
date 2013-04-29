using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemcachedSharp.Commands
{
    class PluralRetrievalObservable : IObservable<MemcachedItem>
    {
        readonly IPool<MemcachedConnection> _pool;
        readonly Func<PluralRetrievalCommand> _commandFactory;

        public PluralRetrievalObservable(IPool<MemcachedConnection> pool, Func<PluralRetrievalCommand> commandFactory)
        {
            _pool = pool;
            _commandFactory = commandFactory;
        }

        public IDisposable Subscribe(IObserver<MemcachedItem> observer)
        {
            var token = new RetrievalToken { Observer = observer };
            var command = _commandFactory();
            command.OnItem += item =>
            {
                var o = token.Observer;
                if (o != null) o.OnNext(item);
            };
            Execute(command, token);
            return token;
        }

        private async void Execute(PluralRetrievalCommand command, RetrievalToken token)
        {
            IObserver<MemcachedItem> observer;
            try
            {
                using (var conn = await _pool.Borrow())
                {
                    await conn.Item.Execute(command);
                }
            }
            catch (Exception ex)
            {
                observer = token.Observer;
                if (observer != null)
                {
                    observer.OnError(ex);
                }
                return;
            }
            observer = token.Observer;
            if (observer != null) observer.OnCompleted();
        }

        private class RetrievalToken : IDisposable
        {
            public volatile IObserver<MemcachedItem> Observer;

            public void Dispose()
            {
                Observer = null;
            }
        }
    }
}
