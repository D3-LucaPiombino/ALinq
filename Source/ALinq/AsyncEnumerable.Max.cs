using System;
using System.Threading.Tasks;

namespace ALinq
{
    public static partial class AsyncEnumerable 
    {
        public static ValueTask<int> Max(this IAsyncEnumerable<int> enumerable)
        {
            return MaxCore(enumerable, (current, next) => next > current);
        }

        public static async ValueTask<int> Max(this IAsyncEnumerable<int?> enumerable)
        {
            return (await MaxCore(enumerable, (current, next) => next.HasValue && next.Value > current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<long> Max(this IAsyncEnumerable<long> enumerable)
        {
            return MaxCore(enumerable, (current, next) => next > current);
        }

        public static async ValueTask<long> Max(this IAsyncEnumerable<long?> enumerable)
        {
            return (await MaxCore(enumerable, (current, next) => next.HasValue && next.Value > current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<float> Max(this IAsyncEnumerable<float> enumerable)
        {
            return MaxCore(enumerable, (current, next) => next > current);
        }

        public static async ValueTask<float> Max(this IAsyncEnumerable<float?> enumerable)
        {
            return (await MaxCore(enumerable, (current, next) => next.HasValue && next.Value > current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<double> Max(this IAsyncEnumerable<double> enumerable)
        {
            return MaxCore(enumerable, (current, next) => next > current);
        }

        public static async ValueTask<double> Max(this IAsyncEnumerable<double?> enumerable)
        {
            return (await MaxCore(enumerable, (current, next) => next.HasValue && next.Value > current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<decimal> Max(this IAsyncEnumerable<decimal> enumerable)
        {
            return MaxCore(enumerable, (current, next) => next > current);
        }

        public static async ValueTask<decimal> Max(this IAsyncEnumerable<decimal?> enumerable)
        {
            return (await MaxCore(enumerable, (current, next) => next.HasValue && next.Value > current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<int> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<int>> selector)
        {
            return MaxCore(enumerable.Select(selector), (current, next) => next > current);
        }

        public static async ValueTask<int> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<int?>> selector)
        {
            return (await MaxCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value > current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<long> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<long>> selector)
        {
            return MaxCore(enumerable.Select(selector), (current, next) => next > current);
        }

        public static async ValueTask<long> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<long?>> selector)
        {
            return (await MaxCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value > current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<float> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<float>> selector)
        {
            return MaxCore(enumerable.Select(selector), (current, next) => next > current);
        }

        public static async ValueTask<float> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<float?>> selector)
        {
            return (await MaxCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value > current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<double> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<double>> selector)
        {
            return MaxCore(enumerable.Select(selector), (current, next) => next > current);
        }

        public static async ValueTask<double> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<double?>> selector)
        {
            return (await MaxCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value > current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<decimal> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<decimal>> selector)
        {
            return MaxCore(enumerable.Select(selector), (current, next) => next > current);
        }

        public static async ValueTask<decimal> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<decimal?>> selector)
        {
            return (await MaxCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value > current).ConfigureAwait(false)).Value;
        }

        private static async ValueTask<T> MaxCore<T>(this IAsyncEnumerable<T> enumerable, Func<T, T, bool> comparerFunc)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (comparerFunc == null) throw new ArgumentNullException("comparerFunc");

            var setOnce = false;
            var accumulator = default(T);


            await enumerable.ForEach((value,_) =>
            {
                if (!setOnce)
                {
                    accumulator = value;
                    setOnce = true;
                }
                else
                {
                    if (comparerFunc(accumulator, value))
                    {
                        accumulator = value;
                    }
                }
            })
            .ConfigureAwait(false);


            return accumulator;
        }




        public static ValueTask<int> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, int> selector)
        {
            return MaxCore(enumerable.Select(selector), (current, next) => next > current);
        }

        public static async ValueTask<int> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, int?> selector)
        {
            return (await MaxCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value > current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<long> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, long> selector)
        {
            return MaxCore(enumerable.Select(selector), (current, next) => next > current);
        }

        public static async ValueTask<long> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, long?> selector)
        {
            return (await MaxCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value > current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<float> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, float> selector)
        {
            return MaxCore(enumerable.Select(selector), (current, next) => next > current);
        }

        public static async ValueTask<float> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, float?> selector)
        {
            return (await MaxCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value > current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<double> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, double> selector)
        {
            return MaxCore(enumerable.Select(selector), (current, next) => next > current);
        }

        public static async ValueTask<double> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, double?> selector)
        {
            return (await MaxCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value > current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<decimal> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, decimal> selector)
        {
            return MaxCore(enumerable.Select(selector), (current, next) => next > current);
        }

        public static async ValueTask<decimal> Max<T>(this IAsyncEnumerable<T> enumerable, Func<T, decimal?> selector)
        {
            return (await MaxCore(enumerable.Select(selector), (current, next) => next.HasValue && next.Value > current).ConfigureAwait(false)).Value;
        }
    }
}