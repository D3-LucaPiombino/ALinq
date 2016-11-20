using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ALinq
{
    public sealed class ConcurrentAsyncProducer<T>
    {
        private readonly Func<T, ValueTask> notificationFunc;
     
        public ValueTask Yield(T item)
        {
            return notificationFunc(item);
        }

        internal ConcurrentAsyncProducer(Func<T, ValueTask> notificationFunc, CancellationToken cancellationToken)
        {
            this.notificationFunc = notificationFunc ?? throw new ArgumentNullException("notificationFunc");
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