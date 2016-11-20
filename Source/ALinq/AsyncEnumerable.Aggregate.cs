using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static ValueTask<TSource> Aggregate<TSource>(
            this IAsyncEnumerable<TSource> enumerable,
            Func<TSource, TSource, ValueTask<TSource>> aggregationFunc
        )
        {
            return Aggregate(enumerable, default(TSource), aggregationFunc, result => result);
        }

        public static ValueTask<TSource> Aggregate<TSource>(
            this IAsyncEnumerable<TSource> enumerable,
            Func<TSource, TSource, TSource> aggregationFunc
        )
        {
            return Aggregate(enumerable, default(TSource), aggregationFunc, result => result);
        }

        public static ValueTask<TAccumulate> Aggregate<TSource, TAccumulate>(
            this IAsyncEnumerable<TSource> enumerable,
            TAccumulate seed,
            Func<TAccumulate, TSource, ValueTask<TAccumulate>> aggregationFunc
        )
        {
            return Aggregate(enumerable, seed, aggregationFunc, result => result);
        }

        public static ValueTask<TAccumulate> Aggregate<TSource, TAccumulate>(
            this IAsyncEnumerable<TSource> enumerable,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> aggregationFunc
        )
        {
            return Aggregate(enumerable, seed, aggregationFunc, result => result);
        }

        public static async ValueTask<TResult> Aggregate<TSource, TAccumulate, TResult>(
            this IAsyncEnumerable<TSource> enumerable,
            TAccumulate seed,
            Func<TAccumulate, TSource, ValueTask<TAccumulate>> aggregationFunc,
            Func<TAccumulate, ValueTask<TResult>> resultSelector
        )
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (aggregationFunc == null) throw new ArgumentNullException("aggregationFunc");
            if (resultSelector == null) throw new ArgumentNullException("resultSelector");

            var accumulator = seed;

            await enumerable.ForEach(async item =>
            {
                accumulator = await aggregationFunc(accumulator, item).ConfigureAwait(false);
            }).ConfigureAwait(false);

            return await resultSelector(accumulator).ConfigureAwait(false);
        }

        public static async ValueTask<TResult> Aggregate<TSource, TAccumulate, TResult>(
            this IAsyncEnumerable<TSource> enumerable,
            TAccumulate seed,
            Func<TAccumulate, TSource, ValueTask<TAccumulate>> aggregationFunc,
            Func<TAccumulate, TResult> resultSelector
        )
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (aggregationFunc == null) throw new ArgumentNullException("aggregationFunc");
            if (resultSelector == null) throw new ArgumentNullException("resultSelector");

            var accumulator = seed;

            await enumerable.ForEach(async item =>
            {
                accumulator = await aggregationFunc(accumulator, item).ConfigureAwait(false);
            }).ConfigureAwait(false);

            return resultSelector(accumulator);
        }

        public static async ValueTask<TResult> Aggregate<TSource, TAccumulate, TResult>(
            this IAsyncEnumerable<TSource> enumerable,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> aggregationFunc,
            Func<TAccumulate, ValueTask<TResult>> resultSelector
        )
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (aggregationFunc == null) throw new ArgumentNullException("aggregationFunc");
            if (resultSelector == null) throw new ArgumentNullException("resultSelector");

            var accumulator = seed;

            await enumerable.ForEach((item, _) => accumulator = aggregationFunc(accumulator, item)).ConfigureAwait(false);

            return await resultSelector(accumulator).ConfigureAwait(false);
        }

        public static async ValueTask<TResult> Aggregate<TSource, TAccumulate, TResult>(
            this IAsyncEnumerable<TSource> enumerable,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> aggregationFunc,
            Func<TAccumulate, TResult> resultSelector
        )
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (aggregationFunc == null) throw new ArgumentNullException("aggregationFunc");
            if (resultSelector == null) throw new ArgumentNullException("resultSelector");

            var accumulator = seed;

            await enumerable.ForEach((item, _) => accumulator = aggregationFunc(accumulator, item)).ConfigureAwait(false);

            return resultSelector(accumulator);
        }
    }
}