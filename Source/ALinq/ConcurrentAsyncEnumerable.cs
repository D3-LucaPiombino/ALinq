using System;
using System.Threading.Tasks;

namespace ALinq
{
    internal class ConcurrentAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly Func<ConcurrentAsyncProducer<T>, Task> producerFunc;

        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetEnumerator()
        {
            return new ConcurrentAsyncEnumerator<T>(producerFunc);
        }

        IAsyncEnumerator IAsyncEnumerable.GetEnumerator()
        {
            return new ConcurrentAsyncEnumerator<T>(producerFunc);
        }

        internal ConcurrentAsyncEnumerable(Func<ConcurrentAsyncProducer<T>, Task> producerFunc)
        {
            if (producerFunc == null) throw new ArgumentNullException("producerFunc");
            this.producerFunc = producerFunc;
        }
    }


    internal class EmptyAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private class EmptyAsyncEnumerator : IAsyncEnumerator<T>
        {
            public T Current { get { throw new InvalidOperationException("Cannot call Current on an empty enumerable."); } }

            object IAsyncEnumerator.Current => Current;

            public Task<bool> MoveNext()
            {
                return Task.FromResult(false);
            }
        }

        private static EmptyAsyncEnumerator _empty = new EmptyAsyncEnumerator();

        public IAsyncEnumerator<T> GetEnumerator()
        {
            return _empty;
        }

        IAsyncEnumerator IAsyncEnumerable.GetEnumerator()
        {
            return _empty;
        }

        private EmptyAsyncEnumerable()
        {

        }

        public static IAsyncEnumerable<T> Instance = new EmptyAsyncEnumerable<T>();
    }
}