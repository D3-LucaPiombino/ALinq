using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> enumerable,Func<T,Task<bool>> predicate)
        {
            return Where(enumerable, (item, index) => predicate(item));
        }

        public static IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            return Where(enumerable, (item, index) => predicate(item));
        }

        public static IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> enumerable,Func<T,int,Task<bool>> predicate)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (predicate == null) throw new ArgumentNullException("predicate");

            return Create<T>(async producer =>
            {
                await enumerable.ForEach(async state =>
                {
                    if (await predicate(state.Item, (int)state.Index).ConfigureAwait(false))
                    {
                        await producer.Yield(state.Item).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);
            });
        }

        public static IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> enumerable, Func<T, int, bool> predicate)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (predicate == null) throw new ArgumentNullException("predicate");

            return Create<T>(async producer =>
            {
                await enumerable.ForEach(async (T item, long index)=>
                {
                    if (predicate(item, (int)index))
                    {
                        await producer.Yield(item).ConfigureAwait(false);
                    }
                })
                .ConfigureAwait(false);
            });
        }
    }
}