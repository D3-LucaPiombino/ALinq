using System;
using System.Threading.Tasks;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static async Task<T> Last<T>(this IAsyncEnumerable<T> enumerable)
        {
            return await First(enumerable.Reverse(), item => true).ConfigureAwait(false);
        }

        public static async Task<T> Last<T>(this IAsyncEnumerable<T> enumerable, Func<T, Task<bool>> predicate)
        {
            return await First(enumerable.Reverse(), predicate).ConfigureAwait(false);
        }

        public static async Task<T> Last<T>(this IAsyncEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            return await First(enumerable.Reverse(), predicate).ConfigureAwait(false);
        }

        public static Task<T> LastOrDefault<T>(this IAsyncEnumerable<T> enumerable)
        {
            return FirstOrDefault(enumerable.Reverse());
        }

        public static Task<T> LastOrDefault<T>(this IAsyncEnumerable<T> enumerable, Func<T, Task<bool>> predicate)
        {
            return FirstOrDefault(enumerable.Reverse(), predicate);
        }

        public static Task<T> LastOrDefault<T>(this IAsyncEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            return FirstOrDefault(enumerable.Reverse(), predicate);
        }
    }
}