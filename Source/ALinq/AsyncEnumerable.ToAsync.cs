using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<T> ToAsync<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");

            return Create<T>(async producer =>
            {
                foreach (var item in enumerable)
                {
                    await producer.Yield(item).ConfigureAwait(false);
                }
            });
        }

        public static IAsyncEnumerable<T> ToAsync<T>(this IEnumerable<Task<T>> enumerable)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");

            return ToAsync(enumerable.GetEnumerator);
        }

        public static IAsyncEnumerable<T> ToAsync<T>(Func<IEnumerator<Task<T>>> enumeratorFactory)
        {
            if (enumeratorFactory == null) throw new ArgumentNullException("enumeratorFactory");

            return new AsyncEnumerableConverter<T>(enumeratorFactory);
        }

        public static IAsyncEnumerable<T> ToAsync<T>(this IObservable<T> observable)
        {
            if (observable == null) throw new ArgumentNullException("observable");

            return Create<T>(async producer => {

                var converter = new ConversionObserver<T>(producer);
                {
                    using (observable.Subscribe(converter))
                    {
                        await converter.WaitForCompletion().ConfigureAwait(false);
                    }
                }
            });
        }


        public struct Maybe<T>
        {
            public bool HasValue;
            public T Value;
        }

        public static IEnumerable<Task<Maybe<T>>> ToEnumerable<T>(this IAsyncEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException("source");
            var enumerator = source.GetEnumerator();
            using (enumerator as IDisposable)
            {
                while (true)
                {
                    yield return enumerator.MoveNext()
                        .ContinueWith(
                            (t, state) =>
                            {
                                if (!t.Result)
                                    return new Maybe<T>();
                                return new Maybe<T>
                                {
                                    HasValue = true,
                                    Value = ((IAsyncEnumerator<T>)state).Current
                                };
                            },
                            enumerator
                        );
                }
            }
        }
    }
}