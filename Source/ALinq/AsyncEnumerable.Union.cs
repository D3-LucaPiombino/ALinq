using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<T> Union<T>(this IAsyncEnumerable<T> first, IAsyncEnumerable<T> second)
        {
            return Union(first, second, EqualityComparer<T>.Default);
        }

        public static IAsyncEnumerable<T> Union<T>(this IAsyncEnumerable<T> first, IAsyncEnumerable<T> second, IEqualityComparer<T> comparer)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (second == null) throw new ArgumentNullException("second");
            if (comparer == null) throw new ArgumentNullException("comparer");

            return Create<T>(async producer =>
            {
                var set = new ConcurrentDictionary<T,bool>(comparer);

                await first.ForEach(async item =>
                {
                    if (set.TryAdd(item,true))
                    {
                        await producer.Yield(item).ConfigureAwait(false);
                    }
                })
                .ConfigureAwait(false);

                await second.ForEach(async item =>
                {
                    if (set.TryAdd(item, true))
                    {
                        await producer.Yield(item).ConfigureAwait(false);
                    }
                })
                .ConfigureAwait(false);
            });
        }
    }
}