using System;
using System.Threading;
using System.Threading.Tasks;

namespace ALinq
{
    public sealed class ConcurrentAsyncProducer<T>
    {
        private readonly Func<T,Task> notificationFunc;
     
        public async Task Yield(T item)
        {
            await notificationFunc(item).ConfigureAwait(false);
        }

        internal ConcurrentAsyncProducer(Func<T,Task> notificationFunc, CancellationToken cancellationToken)
        {
            if (notificationFunc == null) throw new ArgumentNullException("notificationFunc");

            this.notificationFunc = notificationFunc;
            CancellationToken = cancellationToken;
        }

        public CancellationToken CancellationToken { get; private set; }

        public bool EnumerationCancelled
        {
            get
            {
                return CancellationToken.IsCancellationRequested;
            }
        }

        public void ThrowIfCancellationRequested()
        {
            CancellationToken.ThrowIfCancellationRequested();
        }
    }
}