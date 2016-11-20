using System;
using System.Threading.Tasks;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static async ValueTask<T> Single<T>(this IAsyncEnumerable<T> enumerable)
        {
            return await Single(enumerable, item => true).ConfigureAwait(false);
        }

        public static async ValueTask<T> Single<T>(this IAsyncEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (predicate == null) throw new ArgumentNullException("predicate");

            var result = default(T);
            var counter = 0;

            await enumerable.ForEach(state =>
            {
                if (predicate(state.Item))
                {
                    counter++;
                    result = state.Item;

                    if (counter > 1)
                    {
                        state.Break();
                    }
                }
            }).ConfigureAwait(false);

            if (counter == 0)
            {
                throw new InvalidOperationException("Sequence contains no matching element");
            }

            if (counter == 1)
            {
                return result;
            }

            throw new InvalidOperationException("Sequence contains more than one element");
        }
        public static async ValueTask<T> Single<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<bool>> predicate)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (predicate == null) throw new ArgumentNullException("predicate");

            var result = default(T);
            var counter = 0;

            await enumerable.ForEach(async state =>
            {
                if (await predicate(state.Item).ConfigureAwait(false))
                {
                    counter++;
                    result = state.Item;

                    if (counter > 1)
                    {
                        state.Break();
                    }
                }
            }).ConfigureAwait(false);

            if (counter == 0)
            {
                throw new InvalidOperationException("Sequence contains no matching element");
            }

            if (counter == 1)
            {
                return result;
            }

            throw new InvalidOperationException("Sequence contains more than one element");
        }

        public static async ValueTask<T> SingleOrDefault<T>(this IAsyncEnumerable<T> enumerable)
        {
            return await SingleOrDefault(enumerable, item => true).ConfigureAwait(false);
        }

        public static async ValueTask<T> SingleOrDefault<T>(this IAsyncEnumerable<T> enumerable, Func<T, ValueTask<bool>> predicate)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (predicate == null) throw new ArgumentNullException("predicate");

            var result = default(T);
            var counter = 0;

            await enumerable.ForEach(async state =>
            {
                if (await predicate(state.Item).ConfigureAwait(false))
                {
                    counter++;
                    result = state.Item;

                    if (counter > 1)
                    {
                        state.Break();
                    }
                }
            }).ConfigureAwait(false);


            if (counter == 1)
            {
                return result;
            }

            return default(T);
        }

        public static async ValueTask<T> SingleOrDefault<T>(this IAsyncEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (predicate == null) throw new ArgumentNullException("predicate");

            var result = default(T);
            var counter = 0;

            await enumerable.ForEach(state =>
            {
                if (predicate(state.Item))
                {
                    counter++;
                    result = state.Item;

                    if (counter > 1)
                    {
                        state.Break();
                    }
                }
            })
            .ConfigureAwait(false);


            if (counter == 1)
            {
                return result;
            }

            return default(T);
        }
    }
}