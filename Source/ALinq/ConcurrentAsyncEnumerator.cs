using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace ALinq
{
    public interface IAsyncDisposable : IDisposable
    {
        ValueTask DisposeAsync();
    }


    /// <summary>Provides a reusable object that can be awaited by a consumer and manually completed by a producer.</summary>
    /// <typeparam name="TResult">The type of data being passed from producer to consumer.</typeparam>
    internal class RendezvousAwaitable<TResult> : ICriticalNotifyCompletion
    {
        /// <summary>Sentinel object indicating that the operation has completed prior to OnCompleted being called.</summary>
        private static readonly Action s_completionSentinel = () => Debug.Fail("Completion sentinel should never be invoked");

        /// <summary>
        /// The continuation to invoke when the operation completes, or <see cref="s_completionSentinel"/> if the operation
        /// has completed before OnCompleted is called.
        /// </summary>
        private Action _continuation;
        /// <summary>The exception representing the failed async operation, if it failed.</summary>
        private ExceptionDispatchInfo _error;
        /// <summary>The result of the async operation, if it succeeded.</summary>
        private TResult _result;
#if DEBUG
        private bool _resultSet;
#endif

        /// <summary>true if the producer should invoke the continuation asynchronously; otherwise, false.</summary>
        public bool RunContinuationsAsynchronously { get; set; } = true;

        /// <summary>Gets this instance as an awaiter.</summary>
        public RendezvousAwaitable<TResult> GetAwaiter() => this;

        /// <summary>Whether the operation has already completed.</summary>
        public bool IsCompleted
        {
            get
            {
                Action c = Volatile.Read(ref _continuation);
                Debug.Assert(c == null || c == s_completionSentinel);
                return c != null;
            }
        }

        public TResult GetResult()
        {
            AssertResultConsistency(expectedCompleted: true);

            // Clear out the continuation to prepare for another use
            Debug.Assert(_continuation != null);
            _continuation = null;

            // Propagate any error if there is one, clearing it out first to prepare for reuse.
            // We don't need to clear a result, as result and error are mutually exclusive.
            ExceptionDispatchInfo error = _error;
            if (error != null)
            {
                _error = null;
                error.Throw();
            }

            // The operation completed successfully.  Clear and return the result.
            TResult result = _result;
            _result = default(TResult);
#if DEBUG
            _resultSet = false;
#endif
            return result;
        }

        /// <summary>Set the result of the operation.</summary>
        public void SetResult(TResult result)
        {
            AssertResultConsistency(expectedCompleted: false);
            _result = result;
#if DEBUG
            _resultSet = true;
#endif
            NotifyAwaiter();
        }

        /// <summary>Set that the operation was canceled.</summary>
        public void SetCanceled(CancellationToken token = default(CancellationToken))
        {
            SetException(token.IsCancellationRequested ? new OperationCanceledException(token) : new OperationCanceledException());
        }

        /// <summary>Set the failure for the operation.</summary>
        public void SetException(Exception exception)
        {
            Debug.Assert(exception != null);
            AssertResultConsistency(expectedCompleted: false);

            _error = ExceptionDispatchInfo.Capture(exception);
            NotifyAwaiter();
        }

        /// <summary>Alerts any awaiter that the operation has completed.</summary>
        private void NotifyAwaiter()
        {
            ////Trace("NotifyAwaiter()");
            Action c = _continuation ?? Interlocked.CompareExchange(ref _continuation, s_completionSentinel, null);
            if (c != null)
            {
                //Trace("NotifyAwaiter() - 2");
                Debug.Assert(c != s_completionSentinel);

                if (RunContinuationsAsynchronously)
                {
                    Task.Run(c);
                }
                else
                {
                    c();
                }
            }
        }

        /// <summary>Register the continuation to invoke when the operation completes.</summary>
        public void OnCompleted(Action continuation)
        {
            Debug.Assert(continuation != null);

            Action c = _continuation ?? Interlocked.CompareExchange(ref _continuation, continuation, null);
            if (c != null)
            {
                Debug.Assert(c == s_completionSentinel);
                Task.Run(continuation);
                //continuation();
            }
        }

        /// <summary>Register the continuation to invoke when the operation completes.</summary>
        public void UnsafeOnCompleted(Action continuation) => OnCompleted(continuation);

        [Conditional("DEBUG")]
        private void AssertResultConsistency(bool expectedCompleted)
        {
#if DEBUG
            if (expectedCompleted)
            {
                Debug.Assert(_resultSet ^ (_error != null));
            }
            else
            {
                Debug.Assert(!_resultSet && _error == null);
            }
#endif
        }

        [Conditional("DEBUG")]
        private void Trace(string message)
        {
            System.Diagnostics.Trace.TraceInformation(message);
        }
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
        //private class Signalable<TResult>
        //{
        //    /// <summary>
        //    /// Ensure that we run the continuations asynchronously
        //    /// </summary>
        //    /// <typeparam name="TRes"></typeparam>
        //    private class Tcs<TRes> : TaskCompletionSource<TRes>
        //    {
        //        private static Action<object> _trySetResult = state =>
        //        {
        //            var _this = (Tcs<TRes>)state;
        //            var result = _this._result;
        //            _this._result = default(TRes);
        //            _this.TrySetResult(result);
        //        };

        //        private static Action<object> _trySetException = state =>
        //        {
        //            var _this = (Tcs<TRes>)state;
        //            var exception = _this._exception;
        //            _this._exception = null;
        //            _this.TrySetException(exception);
        //        };

        //        private TRes _result;
        //        private Exception _exception;

        //        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //        public Task TrySetResultAsync(TRes result)
        //        {
        //            _result = result;
        //            return InvokeAsync(_trySetResult);
        //        }

        //        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //        public Task TrySetExceptionAsync(Exception exception)
        //        {
        //            _exception = exception;
        //            return InvokeAsync(_trySetResult);
        //        }

        //        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //        private Task InvokeAsync(Action<object> action)
        //        {
        //            return System.Threading.Tasks.Task
        //                .Factory
        //                .StartNew(
        //                    _trySetResult,
        //                    this,
        //                    CancellationToken.None,
        //                    TaskCreationOptions.PreferFairness,
        //                    TaskScheduler.Default
        //                );
        //        }

        //    }


        //    private Tcs<TResult> _tcs = new Tcs<TResult>();
        //    private string _name;
        //    private static Func<Task<TResult>,object,TResult> _refreshTaskCompletionSource = RefreshTaskCompletionSource;

        //    private static TResult RefreshTaskCompletionSource(Task<TResult> previous, object state)
        //    {
        //        var _this = (Signalable<TResult>)state;
        //        //Trace2($"[{_name}]: Signal received");
        //        Interlocked.Exchange(ref _this._tcs, new Tcs<TResult>());
        //        return previous.Result;
        //    }


        //    [Conditional("DEBUG")]
        //    private void Trace2(string message)
        //    {
        //        TraceInformation($"[{_name}]: {message}");
        //    }

        //    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //    //public Task Signal(TResult value = default(TResult))
        //    //{
        //    //    Trace2($"Signal {value} (enqueue)");
        //    //    // Ensure continuations are run asynchronously
        //    //    return _tcs.TrySetResultAsync(value);
        //    //}

        //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //    public void SignalAndForget(TResult value = default(TResult))
        //    {
        //        _tcs.TrySetResult(value);
        //        //Signal(value);
        //    }

        //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //    public void SignalException(Exception exception)
        //    {
        //        Trace2($"Signal {exception} (enqueue)");
        //        _tcs.TrySetException(exception);
        //        // Ensure continuations are run asynchronously
        //        //_tcs.TrySetExceptionAsync(exception);
        //    }

        //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //    public Task<TResult> WaitForSignal()
        //    {
        //        return _tcs.Task.ContinueWith(_refreshTaskCompletionSource, this);
        //    }

        //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //    public ConfiguredTaskAwaitable<TResult>.ConfiguredTaskAwaiter GetAwaiter()
        //    {
        //        return WaitForSignal()
        //            .ConfigureAwait(false)
        //            .GetAwaiter(); // WHY???
        //    }

        //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //    public ConfiguredTaskAwaitable<TResult> ConfigureAwait(bool captureContext)
        //    {
        //        return WaitForSignal()
        //            .ConfigureAwait(captureContext);
        //    }

        //    public Signalable(string name)
        //    {
        //        _name = name;
        //    }
        //}


        private class Signalable2<TResult> : RendezvousAwaitable<TResult>
        {
            private string _name;

            public Signalable2(string name)
            {
                _name = name;
            }

            [Conditional("DEBUG")]
            private void Trace2(string message)
            {
                TraceInformation($"[{_name}]: {message}");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SignalException(Exception exception)
            {
                Trace2($"Signal {exception} (enqueue)");
                SetException(exception);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SignalAndForget(TResult value = default(TResult))
            {
                SetResult(value);
            }

            
        }

        private static long _idseed;
        private long _id;
        private readonly Func<WeakReference<ConcurrentAsyncEnumerator<T>>, ValueTask> _producerWrapperTaskFactory;
        
        private readonly Signalable2<bool> _emitNextValue;
        private readonly Signalable2<bool> _valueAvailable;
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

        public object Current
        {
            get { return _current; }
        }
        
        public async ValueTask<bool> MoveNext()
        {
            Trace2($"C: MoveNext()");
            if (_producerWrapperTask == null)
            {
                // We need to start the procucer task
                _producerWrapperTask = _producerWrapperTaskFactory(new WeakReference<ConcurrentAsyncEnumerator<T>>(this)).AsTask();
            }


            var hasNext = await MoveProducerStateMachineAndWaitForValue()
                .ConfigureAwait(false); // Run the rest of the state machine without the synchronization context.

            

            if (!hasNext)
            {
                Trace2($"C: No Value is available. Terminate");

                // Marshal exceptions and cancellation back to the consumer.
                // Note that if the consumer bail out early (call Dispose), 
                // this will not be invoked here, instead we await 
                // _producerWrapperTask in the dispose method.
                var producerTask = Interlocked.Exchange(ref _producerWrapperTask, null);
                if (producerTask != null)
                    await producerTask;
                return false;
            }
            Trace2($"C: Value is available");
            return true;
        }

        private async ValueTask<bool> MoveProducerStateMachineAndWaitForValue()
        {
            Trace2($"C: _emitNextValue.SignalAndForget()");
            _emitNextValue.SignalAndForget();
            Trace2($"C: await _valueAvailable");
            return await _valueAvailable;
        }

        private async ValueTask Yield(T value)
        {
            Trace2($"P: Yield {value}");

            if (_disposed)
            {
                throw new YieldCalledAfterEnumerationHasBeenAbandonedException();
            } 

            Trace2("P: Wait mutex");
            // Ensure that if we are waiting on _moveNextSignal no
            // one else can mess with _current
            //await _mutex.WaitAsync().ConfigureAwait(false);
            try
            {
                Trace2("P: Assign value");
                _current = value;
                _valueAvailable.SignalAndForget(true);


                Trace2("P: Wait for _emitNextValue to be signalled to proceed to next value");
                // Now, wait for the consumer to call MoveNext().
                // This will guarantee that _current instance is valid until
                // the consumer(s) are done.
                await _emitNextValue;

                if (_disposed)
                {
                    Trace2("P: Enumerator has been disposed. Dispose producer state machine");
                    // This is safe to call even if we use the task in DisposeAsync() (http://blogs.msdn.com/b/pfxteam/archive/2012/03/25/10287435.aspx)
                    _producerWrapperTask?.TryDispose();
                        

                    // This will force the producer state machine to unwind and complete.
                    // Subsequently the EndOfStream() is invoked by the wrapper around the 
                    // producer task that has been composed in the constructor.
                    throw new EnumerationAbandonedException("Enumeration has been cancelled or abandoned");
                }
                Trace2("P: Consumer requested a new value");
            }
            finally
            {
                Trace2("P: Release mutex");
                //_mutex.Release();
                Trace2("P: Released mutex");
            }
            Trace2("P: Exiting Yield()");
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
            // Duplicated but avoid an additional allocation
            if (_disposed)
                return;

            DisposeAsync()
                .AsTask()
                .WaitAndUnwrapAggregateException();
        }

        public async ValueTask DisposeAsync()
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

        internal ConcurrentAsyncEnumerator(Func<ConcurrentAsyncProducer<T>, ValueTask> producerFunc)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            _id = Interlocked.Increment(ref _idseed);

            _emitNextValue = new Signalable2<bool>($"AsyncEnumerator-{_id}({nameof(_emitNextValue)})")
            {
                //RunContinuationsAsynchronously = false
            };
            _valueAvailable = new Signalable2<bool>($"AsyncEnumerator-{_id}({nameof(_valueAvailable)})")
            {
                //RunContinuationsAsynchronously = true
            };

            _producerWrapperTaskFactory = BuildProducerWrapperFactory(producerFunc);
        }

        private Func<WeakReference<ConcurrentAsyncEnumerator<T>>, ValueTask> BuildProducerWrapperFactory(
            Func<ConcurrentAsyncProducer<T>, ValueTask> producerFunc
        )
        {
            return async weakEnumerator =>
            {
                weakEnumerator.TryGetTarget(out var enumerator);
                Trace2("P: Producer container started");
                var producer = new ConcurrentAsyncProducer<T>(enumerator.Yield, _cancellationToken);

                try
                {
                    // Wait for the consumer to call MoveNext() the first time.
                    // This is only necessary to synchronize the enumerator when the 
                    // producer is started.
                    await enumerator._emitNextValue;
                    Trace2("P: Invoker user producer factory");

                    await producerFunc(producer);

                    Trace2("P: Signal End Of Stream.");

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

                    Trace2("P: Signal End Of Stream (Producer function has been cancelled).");
                    enumerator.EndOfStream();
                }
                catch (Exception e)
                {
                    Trace2("P: Signal End Of Stream (Producer function has thrown an exception).");
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
            //Trace.TraceInformation(message);
            //Console.WriteLine(message);
        }
    }
}