using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
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

            public Maybe(T value) : this()
            {
                Value = value;
                HasValue = true;
            }

            public static Task<Maybe<T>> DefaultTask = Task.FromResult(default(Maybe<T>));
        }

        /// <summary>
        /// Convert an <see cref="IAsyncEnumerable{T}"/> into an infinite IEnumerable&lt;Task&lt;Maybe&lt;T>>>.
        /// The result sequence can be used with foreach
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<Task<Maybe<T>>> ToEnumerable<T>(this IAsyncEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException("source");

            var enumerator = source.GetEnumerator();

            try
            {
                while (true)
                {
                    yield return enumerator
                        .MoveNext()
                        .ContinueWith(ConvertNext<T>, enumerator)
                        .Unwrap();

                    //yield return enumerator
                    //    .MoveNext()
                    //    .ContinueWith(ConvertNext2<T>, enumerator)
                    //    .ContinueWith(SafeDispose, enumerator)
                    //    .Unwrap();

                }
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        private static async Task<Maybe<T>> ConvertNext<T>(Task<bool> task, object state)
        {
            ExceptionDispatchInfo edi = null;
            var _enumerator = (IAsyncEnumerator<T>)state;
            try
            {
                var hasNext = await task;
                if (hasNext)
                    return new Maybe<T>(_enumerator.Current);
            }
            catch (Exception e)
            {
                edi = ExceptionDispatchInfo.Capture(e);
            }
            // First dispose the enumerator.
            // Ensure that if an exception was thrown 
            // by MoveNext() and Dispose throw we do not 
            // swallow it.
            await _enumerator.DisposeAsync(edi?.SourceException);
            edi?.Throw();
            return default(Maybe<T>);
        }

        private static Task<Maybe<T>> SafeDispose<T>(Task<Maybe<T>> task, object state)
        {
            var _enumerator = (IAsyncEnumerator<T>)state;
            if (task.Result.HasValue)
                return task;

            return _enumerator
                .DisposeAsync(task.Exception)
                .ContinueWith(ReturnResult<T>, task);
        }

        private static Maybe<T> ReturnResult<T>(Task disposeTask, object resultTask)
        {
            disposeTask.Wait();
            return ((Task<Maybe<T>>)resultTask).Result;
        }
        private static Maybe<T> ConvertNext2<T>(Task<bool> task, object state)
        {
            var _enumerator = (IAsyncEnumerator<T>)state;
            var hasNext = task.Result;
            if (hasNext)
                return new Maybe<T>(_enumerator.Current);
            return default(Maybe<T>);
        }

        
    }


    

}