using System;
using System.Threading;
using System.Threading.Tasks;

namespace ALinq
{
    public sealed class ConcurrentAsyncProducer<T>
    {
        private readonly Func<T,Task> notificationFunc;
     
        public Task Yield(T item)
        {
            return notificationFunc(item);
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