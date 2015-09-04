using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<T> Distinct<T>(this IAsyncEnumerable<T> enumerable)
        {
            return Distinct<T>(enumerable, EqualityComparer<T>.Default);
        }

        public static IAsyncEnumerable<T> Distinct<T>(this IAsyncEnumerable<T> enumerable,IEqualityComparer<T> comparer)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (comparer == null) throw new ArgumentNullException("comparer");

            var aggregator = new ConcurrentDictionary<T,bool>(comparer);

            return Create<T>(async producer =>
            {
                await enumerable.ForEach(async item =>
                {
                    if (aggregator.TryAdd(item, true))
                    {
                        await producer.Yield(item).ConfigureAwait(false);
                    }
                })
                .ConfigureAwait(false);
            });
        }
    }
}