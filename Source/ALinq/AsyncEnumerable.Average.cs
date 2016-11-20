using System;
using System.Threading.Tasks;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static ValueTask<int> Average(this IAsyncEnumerable<int> enumerable)
        {
            return AverageCore(enumerable, (current, next) => current + next, (sum, counter) => sum / (int)counter);
        }

        public static async ValueTask<int> Average(this IAsyncEnumerable<int?> enumerable)
        {
            return (await AverageCore(enumerable,0, (current, next) => next.HasValue ? current + next.Value : current, (sum, counter) => sum / (int?)counter).ConfigureAwait(false)).Value;
        }

        public static ValueTask<long> Average(this IAsyncEnumerable<long> enumerable)
        {
            return AverageCore(enumerable, (current, next) => current + next, (sum, counter) => sum / counter);
        }

        public static async ValueTask<long> Average(this IAsyncEnumerable<long?> enumerable)
        {
            return (await AverageCore(enumerable,0L, (current, next) => next.HasValue ? current + next.Value : current, (sum, counter) => sum / counter).ConfigureAwait(false)).Value;
        }

        public static ValueTask<float> Average(this IAsyncEnumerable<float> enumerable)
        {
            return AverageCore(enumerable, (current, next) => current + next, (sum, counter) => sum / counter);
        }

        public static async ValueTask<float> Average(this IAsyncEnumerable<float?> enumerable)
        {
            return (await AverageCore(enumerable,0.0f, (current, next) => next.HasValue ? current + next.Value : current, (sum, counter) => sum / counter).ConfigureAwait(false)).Value;
        }

        public static ValueTask<double> Average(this IAsyncEnumerable<double> enumerable)
        {
            return AverageCore(enumerable, (current, next) => current + next, (sum, counter) => sum / counter);
        }

        public static async ValueTask<double> Average(this IAsyncEnumerable<double?> enumerable)
        {
            return (await AverageCore(enumerable,0.0, (current, next) => next.HasValue ? current + next.Value : current, (sum, counter) => sum / counter).ConfigureAwait(false)).Value;
        }

        public static ValueTask<decimal> Average(this IAsyncEnumerable<decimal> enumerable)
        {
            return AverageCore(enumerable, (current, next) => current + next, (sum, counter) => sum / counter);
        }

        public static async ValueTask<decimal> Average(this IAsyncEnumerable<decimal?> enumerable)
        {
            return (await AverageCore(enumerable,0.0m, (current, next) => next.HasValue ? current + next.Value : current, (sum, counter) => sum / counter).ConfigureAwait(false)).Value;
        }

        public static ValueTask<int> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<int>> selector)
        {
            return AverageCore(enumerable.Select(selector), (current, next) => current + next, (sum, counter) => sum / (int)counter);
        }

        public static async ValueTask<int> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<int?>> selector)
        {
            return (await AverageCore(enumerable.Select(selector),0,(current, next) => next.HasValue ? current + next.Value : current, (sum, counter) => sum / (int?)counter).ConfigureAwait(false)).Value;
        }

        public static ValueTask<long> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<long>> selector)
        {
            return AverageCore(enumerable.Select(selector), (current, next) => current + next, (sum, counter) => sum / counter);
        }

        public static async ValueTask<long> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<long?>> selector)
        {
            return (await AverageCore(enumerable.Select(selector),0L, (current, next) => next.HasValue ? current + next.Value : current, (sum, counter) => sum / counter).ConfigureAwait(false)).Value;
        }

        public static ValueTask<float> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<float>> selector)
        {
            return AverageCore(enumerable.Select(selector), (current, next) => current + next, (sum, counter) => sum / counter);
        }

        public static async ValueTask<float> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<float?>> selector)
        {
            return (await AverageCore(enumerable.Select(selector),0.0f, (current, next) => next.HasValue ? current + next.Value : current, (sum, counter) => sum / counter).ConfigureAwait(false)).Value;
        }

        public static ValueTask<double> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<double>> selector)
        {
            return AverageCore(enumerable.Select(selector), (current, next) => current + next, (sum, counter) => sum / counter);
        }

        public static async ValueTask<double> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<double?>> selector)
        {
            return (await AverageCore(enumerable.Select(selector),0.0, (current, next) => next.HasValue ? current + next.Value : current, (sum, counter) => sum / counter).ConfigureAwait(false)).Value;
        }

        public static ValueTask<decimal> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<decimal>> selector)
        {
            return AverageCore(enumerable.Select(selector), (current, next) => current + next, (sum, counter) => sum / counter);
        }

        public static async ValueTask<decimal> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<decimal?>> selector)
        {
            return (await AverageCore(enumerable.Select(selector),0.0m, (current, next) => next.HasValue ? current + next.Value : current, (sum, counter) => sum / counter).ConfigureAwait(false)).Value;
        }

        private static async ValueTask<T> AverageCore<T>(this IAsyncEnumerable<T> enumerable,Func<T, T, T> sumFunction, Func<T, long, T> averageFunction)
        {
            return await AverageCore<T>(enumerable, default(T), sumFunction, averageFunction).ConfigureAwait(false);
        }

        private static async ValueTask<T> AverageCore<T>(this IAsyncEnumerable<T> enumerable,T seed,Func<T, T, T> sumFunction, Func<T, long, T> averageFunction)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (sumFunction == null) throw new ArgumentNullException("sumFunction");
            if (averageFunction == null) throw new ArgumentNullException("averageFunction");

            var counter = 0L;
            var accumulator = seed;

#pragma warning disable 1998
            await enumerable.ForEach(async value =>
            {
                accumulator = sumFunction(accumulator, value);
                counter++;
            }).ConfigureAwait(false);
#pragma warning restore 1998

            return averageFunction(accumulator, counter);
        }








        // synchronous

        public static ValueTask<int> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, int> selector)
        {
            return AverageCore(enumerable.Select(selector), (current, next) => current + next, (sum, counter) => sum / (int)counter);
        }

        public static async ValueTask<int> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, int?> selector)
        {
            return (await AverageCore(enumerable.Select(selector), 0, (current, next) => next.HasValue ? current + next.Value : current, (sum, counter) => sum / (int?)counter).ConfigureAwait(false)).Value;
        }

        public static ValueTask<long> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, long> selector)
        {
            return AverageCore(enumerable.Select(selector), (current, next) => current + next, (sum, counter) => sum / counter);
        }

        public static async ValueTask<long> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, long?> selector)
        {
            return (await AverageCore(enumerable.Select(selector), 0L, (current, next) => next.HasValue ? current + next.Value : current, (sum, counter) => sum / counter).ConfigureAwait(false)).Value;
        }

        public static ValueTask<float> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, float> selector)
        {
            return AverageCore(enumerable.Select(selector), (current, next) => current + next, (sum, counter) => sum / counter);
        }

        public static async ValueTask<float> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, float?> selector)
        {
            return (await AverageCore(enumerable.Select(selector), 0.0f, (current, next) => next.HasValue ? current + next.Value : current, (sum, counter) => sum / counter).ConfigureAwait(false)).Value;
        }

        public static ValueTask<double> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, double> selector)
        {
            return AverageCore(enumerable.Select(selector), (current, next) => current + next, (sum, counter) => sum / counter);
        }

        public static async ValueTask<double> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, double?> selector)
        {
            return (await AverageCore(enumerable.Select(selector), 0.0, (current, next) => next.HasValue ? current + next.Value : current, (sum, counter) => sum / counter).ConfigureAwait(false)).Value;
        }

        public static ValueTask<decimal> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, decimal> selector)
        {
            return AverageCore(enumerable.Select(selector), (current, next) => current + next, (sum, counter) => sum / counter);
        }

        public static async ValueTask<decimal> Average<T>(this IAsyncEnumerable<T> enumerable, Func<T, decimal?> selector)
        {
            return (await AverageCore(enumerable.Select(selector), 0.0m, (current, next) => next.HasValue ? current + next.Value : current, (sum, counter) => sum / counter).ConfigureAwait(false)).Value;
        }

    }
}