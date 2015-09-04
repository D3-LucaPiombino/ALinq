using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        private struct AsyncEnumeratorUntypedAdapter : IAsyncEnumerator<object>
        {
            private IAsyncEnumerator _untypedEnumerator;

            public AsyncEnumeratorUntypedAdapter(IAsyncEnumerator untypedEnumerator)
            {
                _untypedEnumerator = untypedEnumerator;
            }

            public object Current => _untypedEnumerator.Current;

            public Task<bool> MoveNext() => _untypedEnumerator.MoveNext();
        }

        public static Task ForEach(this IAsyncEnumerable enumerable, Func<object, AsyncLoopContext<object>, Task> enumerationAction)
        {
            return ForEach(enumerable, loopState => enumerationAction(loopState.Item, loopState));
        }

        public static Task ForEach(this IAsyncEnumerable enumerable, Func<object, long, AsyncLoopContext<object>, Task> enumerationAction)
        {
            return ForEach(enumerable, loopState => enumerationAction(loopState.Item, loopState.Index, loopState));
        }

        public static Task ForEach(this IAsyncEnumerable enumerable, Func<object, long, Task> enumerationAction)
        {
            return ForEach(enumerable, loopState => enumerationAction(loopState.Item, loopState.Index));
        }

        public static Task ForEach(this IAsyncEnumerable enumerable, Func<object, Task> enumerationAction)
        {
            return ForEach(enumerable, loopState => enumerationAction(loopState.Item));
        }

        public static Task ForEach(this IAsyncEnumerable enumerable, Func<AsyncLoopContext<object>, Task> enumerationAction)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (enumerationAction == null) throw new ArgumentNullException("enumerationAction");

            var enumerator = enumerable.GetEnumerator();
            var adapter = new AsyncEnumeratorUntypedAdapter(enumerator);

            return ForEachCore(adapter, enumerationAction);
        }

        public static Task ForEach(this IAsyncEnumerable enumerable, Action<AsyncLoopContext<object>> enumerationAction)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (enumerationAction == null) throw new ArgumentNullException("enumerationAction");

            var enumerator = enumerable.GetEnumerator();
            var adapter = new AsyncEnumeratorUntypedAdapter(enumerator);

            return ForEachCore(adapter, enumerationAction);
        }


        public static Task ForEach<T>(this IAsyncEnumerable<T> enumerable, Func<T, AsyncLoopContext<T>, Task> enumerationAction)
        {
            return ForEach(enumerable, loopState => enumerationAction(loopState.Item, loopState));
        }

        public static Task ForEach<T>(this IAsyncEnumerable<T> enumerable, Func<T, long, AsyncLoopContext<T>, Task> enumerationAction)
        {
            return ForEach(enumerable, loopState => enumerationAction(loopState.Item, loopState.Index, loopState));
        }

        public static Task ForEach<T>(this IAsyncEnumerable<T> enumerable, Func<T, long, Task> enumerationAction)
        {
            return ForEach(enumerable, loopState => enumerationAction(loopState.Item, loopState.Index));
        }

        public static Task ForEach<T>(this IAsyncEnumerable<T> enumerable, Action<T, long> enumerationAction)
        {
            return ForEach(enumerable, loopState => enumerationAction(loopState.Item, loopState.Index));
        }

        public static Task ForEach<T>(this IAsyncEnumerable<T> enumerable, Func<T, Task> enumerationAction)
        {
            return ForEach(enumerable, loopState => enumerationAction(loopState.Item));
        }

        public static Task ForEach<T>(this IAsyncEnumerable<T> enumerable, Func<AsyncLoopContext<T>, Task> enumerationAction)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (enumerationAction == null) throw new ArgumentNullException("enumerationAction");
            return ForEachCore(enumerable.GetEnumerator(), enumerationAction);
        }

        public static Task ForEach<T>(this IAsyncEnumerable<T> enumerable, Action<AsyncLoopContext<T>> enumerationAction)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (enumerationAction == null) throw new ArgumentNullException("enumerationAction");
            return ForEachCore(enumerable.GetEnumerator(), enumerationAction);
        }

        private static async Task ForEachCore<T>(
            this IAsyncEnumerator<T> enumerator,
            Action<AsyncLoopContext<T>> enumerationAction
        )
        {
            if (enumerationAction == null) throw new ArgumentNullException("enumerationAction");

            var index = 0L;
            var loopState = new AsyncLoopContext<T>();

            ExceptionDispatchInfo edi = null;

            try
            {
                while (await enumerator.MoveNext().ConfigureAwait(false))
                {
                    loopState.Item = enumerator.Current;
                    loopState.Index = index;
                    try
                    {
                        enumerationAction(loopState);
                    }
                    catch (Exception e)
                    {
                        edi = ExceptionDispatchInfo.Capture(e);
                        break;
                    }

                    if (loopState.WasBreakCalled)
                    {
                        break;
                    }

                    index++;
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            edi?.Throw();
        }

        private static async Task ForEachCore<T>(
            this IAsyncEnumerator<T> enumerator,
            Func<AsyncLoopContext<T>, Task> enumerationAction
        )
        {
            if (enumerationAction == null) throw new ArgumentNullException("enumerationAction");

            var index = 0L;
            var loopState = new AsyncLoopContext<T>();

            ExceptionDispatchInfo edi = null;

            try
            {
                while (await enumerator.MoveNext().ConfigureAwait(false))
                {
                    loopState.Item = enumerator.Current;
                    loopState.Index = index;
                    try
                    {
                        await enumerationAction(loopState).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        edi = ExceptionDispatchInfo.Capture(e);
                        break;
                    }

                    if (loopState.WasBreakCalled)
                    {
                        break;
                    }

                    index++;
                }
            }
            finally
            {
                await enumerator.DisposeAsync(edi?.SourceException);
            }

            edi?.Throw();
        }

        public static Task ForEach<T>(this IAsyncEnumerable<T> enumerable, ConcurrentAsyncProducer<T> producer)
        {
            return ForEach(enumerable, loopState => producer.Yield(loopState.Item));
        }

    }
}