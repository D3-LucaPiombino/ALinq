﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static async ValueTask<IDictionary<TKey, TSource>> ToDictionary<TSource, TKey>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, ValueTask<TKey>> keySelector,
            IEqualityComparer<TKey> comparer = null
        )
        {
            return await ToDictionary(
                source, 
                keySelector,
                s => new ValueTask<TSource>(s), 
                comparer
            )
            .ConfigureAwait(false);
        }

        public static async ValueTask<IDictionary<TKey, TElement>> ToDictionary<TSource, TKey, TElement>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, ValueTask<TElement>> elementSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            return await ToDictionary(
                source,
                s => new ValueTask<TKey>(keySelector(s)),
                elementSelector,
                comparer
            )
            .ConfigureAwait(false);
        }

        public static async ValueTask<IDictionary<TKey, TElement>> ToDictionary<TSource, TKey, TElement>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, ValueTask<TKey>> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            return await ToDictionary(
                source,
                keySelector,
                s => new ValueTask<TElement>(elementSelector(s)),
                comparer
            )
            .ConfigureAwait(false);
        }

        public static async ValueTask<IDictionary<TKey, TElement>> ToDictionary<TSource, TKey, TElement>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            return await ToDictionary(
                source, 
                s => new ValueTask<TKey>(keySelector(s)), 
                s => new ValueTask<TElement>(elementSelector(s)),
                comparer
            )
            .ConfigureAwait(false);
        }

        // ---

        public static async ValueTask<IDictionary<TKey, TElement>> ToDictionary<TSource, TKey, TElement>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, ValueTask<TKey>> keySelector,
	        Func<TSource, ValueTask<TElement>> elementSelector,
	        IEqualityComparer<TKey> comparer = null)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (keySelector == null) throw new ArgumentNullException("keySelector");
            if (elementSelector == null) throw new ArgumentNullException("elementSelector");
            if (comparer == null) throw new ArgumentNullException("comparer");

            var result = new Dictionary<TKey, TElement>(comparer ?? EqualityComparer<TKey>.Default);

            await source.ForEach(async item =>
            {
                var key     = await keySelector(item).ConfigureAwait(false);
                var value   = await elementSelector(item).ConfigureAwait(false);

                result.Add(key,value);
            }).ConfigureAwait(false);

            return result;
        }
    }
}