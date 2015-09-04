using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<T> Intersect<T>(this IAsyncEnumerable<T> first, IAsyncEnumerable<T> second)
        {
            return Intersect(first, second, EqualityComparer<T>.Default);
        }

        public static IAsyncEnumerable<T> Intersect<T>(this IAsyncEnumerable<T> first, IAsyncEnumerable<T> second, IEqualityComparer<T> comparer)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (second == null) throw new ArgumentNullException("second");
            if (comparer == null) throw new ArgumentNullException("comparer");

            return Create<T>(async producer =>
            {
                var set = new ConcurrentDictionary<T,bool>(comparer);
                await second.ForEach((item,_) => set.TryAdd(item, true)).ConfigureAwait(false);

                await first.ForEach(async item =>
                {
                    if (!set.TryAdd(item, true))
                    {
                        await producer.Yield(item).ConfigureAwait(false);
                    }
                })
                .ConfigureAwait(false);
            });
        }
    }
}