using System;
using System.Threading.Tasks;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static ValueTask<int> Min(this IAsyncEnumerable<int> enumerable)
        {
            return MinCore(enumerable, (current, next) => next < current);
        }

        public static async ValueTask<int> Min(this IAsyncEnumerable<int?> enumerable)
        {
            return (await MinCore(enumerable, (current, next) => next.HasValue && next.Value < current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<long> Min(this IAsyncEnumerable<long> enumerable)
        {
            return MinCore(enumerable, (current, next) => next < current);
        }

        public static async ValueTask<long> Min(this IAsyncEnumerable<long?> enumerable)
        {
            return (await MinCore(enumerable, (current, next) => next.HasValue && next.Value < current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<float> Min(this IAsyncEnumerable<float> enumerable)
        {
            return MinCore(enumerable, (current, next) => next < current);
        }

        public static async ValueTask<float> Min(this IAsyncEnumerable<float?> enumerable)
        {
            return (await MinCore(enumerable, (current, next) => next.HasValue && next.Value < current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<double> Min(this IAsyncEnumerable<double> enumerable)
        {
            return MinCore(enumerable, (current, next) => next < current);
        }

        public static async ValueTask<double> Min(this IAsyncEnumerable<double?> enumerable)
        {
            return (await MinCore(enumerable, (current, next) => next.HasValue && next.Value < current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<decimal> Min(this IAsyncEnumerable<decimal> enumerable)
        {
            return MinCore(enumerable, (current, next) => next < current);
        }

        public static async ValueTask<decimal> Min(this IAsyncEnumerable<decimal?> enumerable)
        {
            return (await MinCore(enumerable, (current, next) => next.HasValue && next.Value < current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<int> Min<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<int>> selector)
        {
            return MinCore(enumerable.Select(selector), (current, next) => next < current);
        }

        public static async ValueTask<int> Min<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<int?>> selector)
        {
            return (await MinCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value < current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<long> Min<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<long>> selector)
        {
            return MinCore(enumerable.Select(selector), (current, next) => next < current);
        }

        public static async ValueTask<long> Min<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<long?>> selector)
        {
            return (await MinCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value < current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<float> Min<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<float>> selector)
        {
            return MinCore(enumerable.Select(selector), (current, next) => next < current);
        }

        public static async ValueTask<float> Min<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<float?>> selector)
        {
            return (await MinCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value < current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<double> Min<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<double>> selector)
        {
            return MinCore(enumerable.Select(selector), (current, next) => next < current);
        }

        public static async ValueTask<double> Min<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<double?>> selector)
        {
            return (await MinCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value < current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<decimal> Min<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<decimal>> selector)
        {
            return MinCore(enumerable.Select(selector), (current, next) => next < current);
        }

        public static async ValueTask<decimal> Min<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<decimal?>> selector)
        {
            return (await MinCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value < current).ConfigureAwait(false)).Value;
        }

        private static async ValueTask<T> MinCore<T>(this IAsyncEnumerable<T> enumerable, Func<T, T, bool> comparerFunc)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (comparerFunc == null) throw new ArgumentNullException("comparerFunc");

            var setOnce     = false;
            var accumulator = default(T);


            await enumerable.ForEach((value,_) =>
            {
                if ( !setOnce )
                {
                    accumulator = value;
                    setOnce = true;
                }
                else
                {
                    if ( comparerFunc(accumulator,value))
                    {
                        accumulator = value;
                    }
                }
            })
            .ConfigureAwait(false);


            return accumulator;
        }



        public static async ValueTask<int> Min<T>(this IAsyncEnumerable<T> enumerable, Func<T, int?> selector)
        {
            return (await MinCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value < current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<long> Min<T>(this IAsyncEnumerable<T> enumerable, Func<T, long> selector)
        {
            return MinCore(enumerable.Select(selector), (current, next) => next < current);
        }

        public static async ValueTask<long> Min<T>(this IAsyncEnumerable<T> enumerable, Func<T, long?> selector)
        {
            return (await MinCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value < current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<float> Min<T>(this IAsyncEnumerable<T> enumerable, Func<T, float> selector)
        {
            return MinCore(enumerable.Select(selector), (current, next) => next < current);
        }

        public static async ValueTask<float> Min<T>(this IAsyncEnumerable<T> enumerable, Func<T, float?> selector)
        {
            return (await MinCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value < current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<double> Min<T>(this IAsyncEnumerable<T> enumerable, Func<T, double> selector)
        {
            return MinCore(enumerable.Select(selector), (current, next) => next < current);
        }

        public static async ValueTask<double> Min<T>(this IAsyncEnumerable<T> enumerable, Func<T, double?> selector)
        {
            return (await MinCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value < current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<decimal> Min<T>(this IAsyncEnumerable<T> enumerable, Func<T, decimal> selector)
        {
            return MinCore(enumerable.Select(selector), (current, next) => next < current);
        }

        public static async ValueTask<decimal> Min<T>(this IAsyncEnumerable<T> enumerable, Func<T, decimal?> selector)
        {
            return (await MinCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value < current).ConfigureAwait(false)).Value;
        }
    }
}