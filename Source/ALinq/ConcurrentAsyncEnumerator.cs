using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ALinq
{
    public interface IAsyncDisposable : IDisposable
    {
        Task DisposeAsync();
    }


    [Serializable]
    public class EnumerationAbandonedException : TaskCanceledException
    {
        public EnumerationAbandonedException() { }
        public EnumerationAbandonedException(string message) : base(message) { }
        public EnumerationAbandonedException(string message, Exception inner) : base(message, inner) { }
        protected EnumerationAbandonedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }


    [Serializable]
    public class YieldCalledAfterEnumerationHasBeenAbandonedException : ObjectDisposedException
    {
        private const string _message =
@"The enumerator consumer has abandoned the enumeration (for example FirstOrDefault()).
This exception is thrown only if Yield is invoked after Dispose (or DisposeAsync) has been called on the enumerator.

Check that the producer implementation:

1. Yield(value) is called in a finally clause
2. If a catch handler with a generic filter (eg. catch{} or catch(Exception){}) is present around a call to Yield.

In either case, you need to check if the enumeration has been cancelled by using the provided 
ConcurrentAsyncProducer.EnumerationCancelled property, invoke ConcurrentAsyncProducer.ThrowIfCancellationRequested() 
or by using directly the ConcurrentAsyncProducer.CancellationToken to partecipate cooperatively in the cancellation
process.

Eg:

var sequence = AsyncEnumerable.Create<int>(async producer =>
{
    try
    {
        while (true)
        {
            try
            {
                await AsyncOperation(producer.CancellationToken);
                await producer.Yield(1);
            }
            catch (OperationCancelledException)
            {
            }
            catch (Exception) 
                // Or if are using C# 6 you can alternatively also use exception 
                // filters and omit the catch above:
                // when(!p.EnumerationCancelled) 
            {
                await producer.Yield(2);
            }
                        
        }
    }
    finally
    {
        if(!producer.EnumerationCancelled)
            await producer.Yield(3);
        
        // This will never be invoked without the explicit 
        // check above
        await ReleaseResourcesAsync();
    }
});
";
        public YieldCalledAfterEnumerationHasBeenAbandonedException() : base("enumerator", _message) { }
        public YieldCalledAfterEnumerationHasBeenAbandonedException(Exception inner) : base(_message, inner) { }
        protected YieldCalledAfterEnumerationHasBeenAbandonedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
    
    internal sealed class ConcurrentAsyncEnumerator<T> : IAsyncEnumerator<T>, IAsyncDisposable
    {
        private class Signalable<TResult>
        {
            /// <summary>
            /// Ensure that we run the continuations asynchronously
            /// </summary>
            /// <typeparam name="TRes"></typeparam>
            private class Tcs<TRes> : TaskCompletionSource<TRes>
            {
                private static Action<object> _trySetResult = state =>
                {
                    var _this = (Tcs<TRes>)state;
                    var result = _this._result;
                    _this._result = default(TRes);
                    _this.TrySetResult(result);
                };

                private static Action<object> _trySetException = state =>
                {
                    var _this = (Tcs<TRes>)state;
                    var exception = _this._exception;
                    _this._exception = null;
                    _this.TrySetException(exception);
                };

                private TRes _result;
                private Exception _exception;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public Task TrySetResultAsync(TRes result)
                {
                    _result = result;
                    return InvokeAsync(_trySetResult);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public Task TrySetExceptionAsync(Exception exception)
                {
                    _exception = exception;
                    return InvokeAsync(_trySetResult);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private Task InvokeAsync(Action<object> action)
                {
                    return System.Threading.Tasks.Task
                        .Factory
                        .StartNew(
                            _trySetResult,
                            this,
                            CancellationToken.None,
                            TaskCreationOptions.PreferFairness,
                            TaskScheduler.Default
                        );
                }

            }


            private Tcs<TResult> _tcs = new Tcs<TResult>();
            private string _name;
            private static Func<Task<TResult>,object,TResult> _refreshTaskCompletionSource = RefreshTaskCompletionSource;

            private static TResult RefreshTaskCompletionSource(Task<TResult> previous, object state)
            {
                var _this = (Signalable<TResult>)state;
                //Trace2($"[{_name}]: Signal received");
                Interlocked.Exchange(ref _this._tcs, new Tcs<TResult>());
                return previous.Result;
            }


            [Conditional("DEBUG")]
            private void Trace2(string message)
            {
                TraceInformation($"[{_name}]: {message}");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Task Signal(TResult value = default(TResult))
            {
                Trace2($"Signal {value} (enqueue)");
                // Ensure continuations are run asynchronously
                return _tcs.TrySetResultAsync(value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SignalAndForget(TResult value = default(TResult))
            {
                _tcs.TrySetResult(value);
                //Signal(value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SignalException(Exception exception)
            {
                Trace2($"Signal {exception} (enqueue)");
                _tcs.TrySetException(exception);
                // Ensure continuations are run asynchronously
                //_tcs.TrySetExceptionAsync(exception);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Task<TResult> WaitForSignal()
            {
                return _tcs.Task.ContinueWith(_refreshTaskCompletionSource, this);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ConfiguredTaskAwaitable<TResult>.ConfiguredTaskAwaiter GetAwaiter()
            {
                return WaitForSignal()
                    .ConfigureAwait(false)
                    .GetAwaiter(); // WHY???
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ConfiguredTaskAwaitable<TResult> ConfigureAwait(bool captureContext)
            {
                return WaitForSignal()
                    .ConfigureAwait(captureContext);
            }

            public Signalable(string name)
            {
                _name = name;
            }
        }

        private static long _idseed;
        private long _id;
        private readonly Func<WeakReference<ConcurrentAsyncEnumerator<T>>, Task> _producerWrapperTaskFactory;
        
        private readonly Signalable<bool> _emitNextValue;
        private readonly Signalable<bool> _valueAvailable;
        private readonly SemaphoreSlim _mutex = new SemaphoreSlim(1);
        private Task _producerWrapperTask;
        private T _current;
        private bool _disposed;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        T IAsyncEnumerator<T>.Current
        {
            get { return _current; }
        }

        object IAsyncEnumerator.Current
        {
            get { return _current; }
        }
        
        async Task<bool> IAsyncEnumerator.MoveNext()
        {
            if (_producerWrapperTask == null)
            {
                // We need to start the procucer task
                _producerWrapperTask = _producerWrapperTaskFactory(new WeakReference<ConcurrentAsyncEnumerator<T>>(this));
            }


            var hasNext = await MoveProducerStateMachineAndWaitForValue()
                .ConfigureAwait(false); // Run the rest of the state machine without the synchronization context.

            if(!hasNext)
            {
                // Marshal exceptions and cancellation back to the consumer.
                // Note that if the consumer bail out early (call Dispose), 
                // this will not be invoked here, instead we await 
                // _producerWrapperTask in the dispose method.
                var producerTask = Interlocked.Exchange(ref _producerWrapperTask, null);
                if (producerTask != null)
                    await producerTask;
                return false;
            }
            return true;
        }

        private Task<bool> MoveProducerStateMachineAndWaitForValue()
        {
            _emitNextValue.SignalAndForget();
            return _valueAvailable.WaitForSignal();
        }

        private async Task Yield(T value)
        {
            Trace2($"Yield {value}");

            if (_disposed)
            {
                throw new YieldCalledAfterEnumerationHasBeenAbandonedException();
            } 

            Trace2("Wait mutex");
            // Ensure that if we are waiting on _moveNextSignal no
            // one else can mess with _current
            await _mutex.WaitAsync().ConfigureAwait(false);
            try
            {
                Trace2("Assign value");
                _current = value;
                _valueAvailable.SignalAndForget(true);


                Trace2("Wait for signal to proceed to next value");
                // Now, wait for the consumer to call MoveNext().
                // This will guarantee that _current instance is valid until
                // the consumer(s) are done.
                await _emitNextValue.WaitForSignal();

                if (_disposed)
                {
                    Trace2("Enumerator has been disposed. Dispose producer state machine");
                    // This is safe to call even if we use the task in DisposeAsync() (http://blogs.msdn.com/b/pfxteam/archive/2012/03/25/10287435.aspx)
                    _producerWrapperTask?.TryDispose();
                        

                    // This will force the producer state machine to unwind and complete.
                    // Subsequently the EndOfStream() is invoked by the wrapper around the 
                    // producer task that has been composed in the constructor.
                    throw new EnumerationAbandonedException("Enumeration has been cancelled or abandoned");
                }

            }
            finally
            {
                _mutex.Release();
            }
        }

        private void EndOfStream()
        {
            _valueAvailable.SignalAndForget();
        }

        private void EndOfStream(Exception e)
        {
            _valueAvailable.SignalException(e);
        }

        public void Dispose()
        {
            DisposeAsync().WaitAndUnwrapAggregateException();
        }

        public async Task DisposeAsync()
        {
            if (_disposed)
                return;

            Trace2("Dispose");
            _disposed = true;
            
            // Cancel the enumeration
            _cancellationTokenSource.Cancel();

            Trace2("Release producer state machine");

            // Wake up the producer if it was stuck in Yield.
            _emitNextValue.SignalAndForget();

            var producerWrapperTask = Interlocked.Exchange(ref _producerWrapperTask, null);
            if (producerWrapperTask != null)
            {
                try
                {
                    // Propagate exceptions
                    await producerWrapperTask.ConfigureAwait(false);
                }
                finally
                {
                    producerWrapperTask.Dispose();
                }
            }

            if(_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Dispose();
            }
                

            Trace2("Disposed");
        }

        internal ConcurrentAsyncEnumerator(Func<ConcurrentAsyncProducer<T>, Task> producerFunc)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            _id = Interlocked.Increment(ref _idseed);

            _emitNextValue = new Signalable<bool>($"AsyncEnumerator-{_id}({nameof(_emitNextValue)})");
            _valueAvailable = new Signalable<bool>($"AsyncEnumerator-{_id}({nameof(_valueAvailable)})");

            _producerWrapperTaskFactory = BuildProducerWrapperFactory(producerFunc);
        }

        private Func<WeakReference<ConcurrentAsyncEnumerator<T>>, Task> BuildProducerWrapperFactory(
            Func<ConcurrentAsyncProducer<T>, Task> producerFunc
        )
        {
            return async weakEnumerator =>
            {
                ConcurrentAsyncEnumerator<T> enumerator;
                weakEnumerator.TryGetTarget(out enumerator);
                Trace2("Producer container started");
                var producer = new ConcurrentAsyncProducer<T>(item => enumerator.Yield(item), _cancellationToken);

                try
                {
                    // Wait for the consumer to call MoveNext() the first time.
                    // This is only necessary to synchronize the enumerator when the 
                    // producer is started.
                    await enumerator._emitNextValue;
                    Trace2("Invoker user produder factory");

                    await producerFunc(producer);

                    enumerator.EndOfStream();
                }
                catch (OperationCanceledException)
                {
                    // The consumer decided to stop the enumeration (by calling Dispose or DisposeAsync).
                    // The idea here is to make sure that any code after the call to Yield that caused 
                    // this exception is not executed.
                    // Throwing an exception is the only way to ensure that resources are cleaned up 
                    // correctly (try/catch/finally).
                    // 
                    // In the the case where 
                    // * The consumer cancel the enumeration by calling Dispose (or DisposeAsync)
                    // * The consumer invoke Yield again
                    // * Yield will then throw an ObjectDisposedException to signal 
                    // that something is not working as intended.
                    //
                    // But there are cases that are not easily handled.
                    // If the producer code has a "catch all" clause (catch {} or catch(Exception){})
                    // then the producer need to explicitely handle cooperatively to stop the execution
                    // correctly.

                    enumerator.EndOfStream();
                }
                catch (Exception e)
                {
                    // There was an exception (other than EnumerationAbandonedException) 
                    // in the producer code.
                    enumerator.EndOfStream(e);
                    // Ensure that this is propagated in Dispose too
                    throw;
                }

            };
        }

        [Conditional("DEBUG")]
        private void Trace2(string message)
        {
            TraceInformation($"[AsyncEnumerator-{_id}]: {message}");
        }

        private static void TraceInformation(string message)
        {
            Trace.TraceInformation(message);
        }
    }
}