using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer,IAsyncEnumerable<TInner> inner,
                                                                                    Func<TOuter, Task<TKey>> outerKeySelector,
                                                                                    Func<TInner, Task<TKey>> innerKeySelector,
                                                                                    Func<TOuter, TInner, Task<TResult>> resultSelector)
        {
            return Join(outer, inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<TKey>.Default);
        }

        public static IAsyncEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer,IAsyncEnumerable<TInner> inner,
            Func<TOuter, Task<TKey>> outerKeySelector,
            Func<TInner, Task<TKey>> innerKeySelector,
            Func<TOuter, TInner, Task<TResult>> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            if (outer == null) throw new ArgumentNullException("outer");
            if (inner == null) throw new ArgumentNullException("inner");
            if (outerKeySelector == null) throw new ArgumentNullException("outerKeySelector");
            if (innerKeySelector == null) throw new ArgumentNullException("innerKeySelector");
            if (resultSelector == null) throw new ArgumentNullException("resultSelector");
            if (comparer == null) throw new ArgumentNullException("comparer");

            var innerDictionary = new ConcurrentDictionary<TKey, List<TInner>>(comparer);

            return Create<TResult>(async producer =>
            {
                // Fill the inner set:
                await inner.ForEach(async state =>
                {
                    var innerKey = await innerKeySelector(state.Item).ConfigureAwait(false);

                    innerDictionary.AddOrUpdate(innerKey,
                        k => new List<TInner>() { state.Item },
                        (k, current) => {
                            current.Add(state.Item);
                            return current;
                        }
                    );

                })
                .ConfigureAwait(false);

                await outer.ForEach(async (TOuter item, long _) =>
                {
                    var outerKey = await outerKeySelector(item).ConfigureAwait(false);
                    List<TInner> innerList;
                    if (innerDictionary.TryGetValue(outerKey, out innerList))
                    {
                        foreach( var innerItem in innerList )
                        {
                            var result = await resultSelector(item, innerItem).ConfigureAwait(false);
                            await producer.Yield(result).ConfigureAwait(false);
                        }
                    }
                }).ConfigureAwait(false);
            });
        }
    }
}