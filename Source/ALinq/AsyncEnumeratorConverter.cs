using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ALinq
{
    internal sealed class AsyncEnumeratorConverter<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<ValueTask<T>>   enumerator;
        private T                               current;

        object IAsyncEnumerator.Current => current;

        T IAsyncEnumerator<T>.Current => current;

        public async ValueTask<bool> MoveNext()
        {
            if (enumerator.MoveNext())
            {
                //using (
                //    var task = enumerator.Current
                //)
                var task = enumerator.Current;
                {
                    if (task != null)
                    {
                        current = await task.ConfigureAwait(false);
                        return true;
                    }
                }
            }

            return false;
        }

        internal AsyncEnumeratorConverter(IEnumerator<ValueTask<T>> enumerator)
        {
            this.enumerator = enumerator ?? throw new ArgumentNullException("enumerator");
        }
    }

    
}