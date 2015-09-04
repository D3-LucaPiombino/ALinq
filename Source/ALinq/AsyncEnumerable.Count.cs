using System;
using System.Threading.Tasks;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static Task<int> Count<T>(this IAsyncEnumerable<T> enumerable, Func<T, Task<bool>> filter)
        {
            if (filter == null) throw new ArgumentNullException("filter");
            return Count(enumerable.Where(filter));
        }

        public static Task<int> Count<T>(this IAsyncEnumerable<T> enumerable, Func<T, bool> filter)
        {
            if (filter == null) throw new ArgumentNullException("filter");
            return Count(enumerable.Where(filter));
        }

        public static Task<long> LongCount<T>(this IAsyncEnumerable<T> enumerable, Func<T, Task<bool>> filter)
        {
            if (filter == null) throw new ArgumentNullException("filter");
            return LongCount(enumerable.Where(filter));
        }

        public static Task<long> LongCount<T>(this IAsyncEnumerable<T> enumerable, Func<T, bool> filter)
        {
            if (filter == null) throw new ArgumentNullException("filter");
            return LongCount(enumerable.Where(filter));
        }

        public static async Task<int> Count<T>(this IAsyncEnumerable<T> enumerable)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");

            var counter = 0;
            await enumerable
                .ForEach((T item, long _) => counter++)
                .ConfigureAwait(false);

            return counter;
        }

        public static async Task<long> LongCount<T>(this IAsyncEnumerable<T> enumerable)
        {
            var counter = 0L;
            await enumerable
               .ForEach((T item, long _) => counter++)
               .ConfigureAwait(false);
            return counter;
        }
    }
}