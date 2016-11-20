using System;
using System.Threading.Tasks;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static ValueTask<int> Sum(this IAsyncEnumerable<int> enumerable)
        {
            return SumCore(enumerable, (current, next) => current + next);
        }

        public static async ValueTask<int> Sum(this IAsyncEnumerable<int?> enumerable)
        {
            return (await SumCore(enumerable, 0, (current, next) => next.HasValue ? current + next.Value : current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<long> Sum(this IAsyncEnumerable<long> enumerable)
        {
            return SumCore(enumerable, (current, next) => current + next);
        }

        public static async ValueTask<long> Sum(this IAsyncEnumerable<long?> enumerable)
        {
            return (await SumCore(enumerable, 0L, (current, next) => next.HasValue ? current + next.Value : current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<float> Sum(this IAsyncEnumerable<float> enumerable)
        {
            return SumCore(enumerable, (current, next) => current + next);
        }

        public static async ValueTask<float> Sum(this IAsyncEnumerable<float?> enumerable)
        {
            return (await SumCore(enumerable, 0.0f, (current, next) => next.HasValue ? current + next.Value : current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<double> Sum(this IAsyncEnumerable<double> enumerable)
        {
            return SumCore(enumerable, (current, next) => current + next);
        }

        public static async ValueTask<double> Sum(this IAsyncEnumerable<double?> enumerable)
        {
            return (await SumCore(enumerable, 0.0, (current, next) => next.HasValue ? current + next.Value : current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<decimal> Sum(this IAsyncEnumerable<decimal> enumerable)
        {
            return SumCore(enumerable, (current, next) => current + next);
        }

        public static async ValueTask<decimal> Sum(this IAsyncEnumerable<decimal?> enumerable)
        {
            return (await SumCore(enumerable, 0.0m, (current, next) => next.HasValue ? current + next.Value : current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<int> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<int>> selector)
        {
            return SumCore(enumerable.Select(selector), (current, next) => current + next);
        }

        public static async ValueTask<int> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<int?>> selector)
        {
            return (await SumCore(enumerable.Select(selector), 0, (current, next) => next.HasValue ? current + next.Value : current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<long> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<long>> selector)
        {
            return SumCore(enumerable.Select(selector), (current, next) => current + next);
        }

        public static async ValueTask<long> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<long?>> selector)
        {
            return (await SumCore(enumerable.Select(selector), 0L, (current, next) => next.HasValue ? current + next.Value : current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<float> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<float>> selector)
        {
            return SumCore(enumerable.Select(selector), (current, next) => current + next);
        }

        public static async ValueTask<float> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<float?>> selector)
        {
            return (await SumCore(enumerable.Select(selector), 0.0f, (current, next) => next.HasValue ? current + next.Value : current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<double> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<double>> selector)
        {
            return SumCore(enumerable.Select(selector), (current, next) => current + next);
        }

        public static async ValueTask<double> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<double?>> selector)
        {
            return (await SumCore(enumerable.Select(selector), 0.0, (current, next) => next.HasValue ? current + next.Value : current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<decimal> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<decimal>> selector)
        {
            return SumCore(enumerable.Select(selector), (current, next) => current + next);
        }

        public static async ValueTask<decimal> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<decimal?>> selector)
        {
            return (await SumCore(enumerable.Select(selector), 0.0m, (current, next) => next.HasValue ? current + next.Value : current).ConfigureAwait(false)).Value;
        }

        private static ValueTask<T> SumCore<T>(this IAsyncEnumerable<T> enumerable,Func<T, T, T> sumFunction)
        {
            return SumCore<T>(enumerable, default(T), sumFunction);
        }

        private static ValueTask<T> SumCore<T>(this IAsyncEnumerable<T> enumerable, T seed,Func<T, T, T> sumFunction)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (sumFunction == null) throw new ArgumentNullException("sumFunction");

#pragma warning disable 1998
            return Aggregate(enumerable,seed,async (a, b) => sumFunction(a, b));
#pragma warning restore 1998
        }




      
        public static ValueTask<int> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, int> selector)
        {
            return SumCore(enumerable.Select(selector), (current, next) => current + next);
        }

        public static async ValueTask<int> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, int?> selector)
        {
            return (await SumCore(enumerable.Select(selector), 0, (current, next) => next.HasValue ? current + next.Value : current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<long> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, long> selector)
        {
            return SumCore(enumerable.Select(selector), (current, next) => current + next);
        }

        public static async ValueTask<long> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, long?> selector)
        {
            return (await SumCore(enumerable.Select(selector), 0L, (current, next) => next.HasValue ? current + next.Value : current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<float> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, float> selector)
        {
            return SumCore(enumerable.Select(selector), (current, next) => current + next);
        }

        public static async ValueTask<float> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, float?> selector)
        {
            return (await SumCore(enumerable.Select(selector), 0.0f, (current, next) => next.HasValue ? current + next.Value : current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<double> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, double> selector)
        {
            return SumCore(enumerable.Select(selector), (current, next) => current + next);
        }

        public static async ValueTask<double> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, double?> selector)
        {
            return (await SumCore(enumerable.Select(selector), 0.0, (current, next) => next.HasValue ? current + next.Value : current).ConfigureAwait(false)).Value;
        }

        public static ValueTask<decimal> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, decimal> selector)
        {
            return SumCore(enumerable.Select(selector), (current, next) => current + next);
        }

        public static async ValueTask<decimal> Sum<T>(this IAsyncEnumerable<T> enumerable, Func<T, decimal?> selector)
        {
            return (await SumCore(enumerable.Select(selector), 0.0m, (current, next) => next.HasValue ? current + next.Value : current).ConfigureAwait(false)).Value;
        }
    }
}