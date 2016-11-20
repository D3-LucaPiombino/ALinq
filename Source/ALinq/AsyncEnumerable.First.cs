using System;
using System.Threading.Tasks;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static async ValueTask<T> First<T>(this IAsyncEnumerable<T> enumerable)
        {
            return await First(enumerable, item => true).ConfigureAwait(false);
        }

        public static async ValueTask<T> First<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<bool>> predicate)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (predicate == null) throw new ArgumentNullException("predicate");

            var result = default(T);
            var found = false;

            await enumerable.ForEach(async state =>
            {
                if (await predicate(state.Item).ConfigureAwait(false))
                {
                    found = true;
                    result = state.Item;
                    state.Break();
                }
            }).ConfigureAwait(false);

            if (found)
            {
                return result;
            }

            throw new InvalidOperationException("Sequence contains no matching element");
        }

        public static async ValueTask<T> First<T>(this IAsyncEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (predicate == null) throw new ArgumentNullException("predicate");

            var result = default(T);
            var found = false;

            await enumerable.ForEach(state =>
            {
                if (predicate(state.Item))
                {
                    found = true;
                    result = state.Item;
                    state.Break();
                }
            })
            .ConfigureAwait(false);

            if (found)
            {
                return result;
            }

            throw new InvalidOperationException("Sequence contains no matching element");
        }


        public static async ValueTask<T> FirstOrDefault<T>(this IAsyncEnumerable<T> enumerable)
        {
            return await FirstOrDefault<T>(enumerable, item => true).ConfigureAwait(false);
        }

        public static async ValueTask<T> FirstOrDefault<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<bool>> predicate)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (predicate == null) throw new ArgumentNullException("predicate");

            var result = default(T);
            var found = false;

            await enumerable.ForEach(async state =>
            {
                if (await predicate(state.Item).ConfigureAwait(false))
                {
                    found = true;
                    result = state.Item;
                    state.Break();
                }
            }).ConfigureAwait(false);

            return found ? result : default(T);
        }

        public static async ValueTask<T> FirstOrDefault<T>(this IAsyncEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (predicate == null) throw new ArgumentNullException("predicate");

            var result = default(T);
            var found = false;

            await enumerable.ForEach(state =>
            {
                if (predicate(state.Item))
                {
                    found = true;
                    result = state.Item;
                    state.Break();
                }
            })
            .ConfigureAwait(false);

            return found ? result : default(T);
        }
    }
}