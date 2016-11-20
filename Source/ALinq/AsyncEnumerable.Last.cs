using System;
using System.Threading.Tasks;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static async ValueTask<T> Last<T>(this IAsyncEnumerable<T> enumerable)
        {
            return await First(enumerable.Reverse(), item => true).ConfigureAwait(false);
        }

        public static async ValueTask<T> Last<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<bool>> predicate)
        {
            return await First(enumerable.Reverse(), predicate).ConfigureAwait(false);
        }

        public static async ValueTask<T> Last<T>(this IAsyncEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            return await First(enumerable.Reverse(), predicate).ConfigureAwait(false);
        }

        public static ValueTask<T> LastOrDefault<T>(this IAsyncEnumerable<T> enumerable)
        {
            return FirstOrDefault(enumerable.Reverse());
        }

        public static ValueTask<T> LastOrDefault<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<bool>> predicate)
        {
            return FirstOrDefault(enumerable.Reverse(), predicate);
        }

        public static ValueTask<T> LastOrDefault<T>(this IAsyncEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            return FirstOrDefault(enumerable.Reverse(), predicate);
        }
    }
}