using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ALinq
{
    internal sealed class AsyncEnumerableConverter<T> : IAsyncEnumerable<T>
    {
        private readonly Func<IEnumerator<ValueTask<T>>> enumeratorFactory;

        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetEnumerator()
        {
            return new AsyncEnumeratorConverter<T>(enumeratorFactory());
        }

        IAsyncEnumerator IAsyncEnumerable.GetEnumerator()
        {
            return new AsyncEnumeratorConverter<T>(enumeratorFactory());
        }

        internal AsyncEnumerableConverter(Func<IEnumerator<ValueTask<T>>> enumeratorFactory)
        {
            this.enumeratorFactory = enumeratorFactory ?? throw new ArgumentNullException("enumeratorFactory");
        }
    }
}