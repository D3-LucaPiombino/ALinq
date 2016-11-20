using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    [StructLayout(LayoutKind.Auto)]
    public struct ConfiguredValueTaskAwaitable
    {
        /// <summary>The wrapped <see cref="ValueTask{TResult}"/>.</summary>
        private readonly ValueTask _value;
        /// <summary>true to attempt to marshal the continuation back to the original context captured; otherwise, false.</summary>
        private readonly bool _continueOnCapturedContext;

        /// <summary>Initializes the awaitable.</summary>
        /// <param name="value">The wrapped <see cref="ValueTask{TResult}"/>.</param>
        /// <param name="continueOnCapturedContext">
        /// true to attempt to marshal the continuation back to the original synchronization context captured; otherwise, false.
        /// </param>
        internal ConfiguredValueTaskAwaitable(ValueTask value, bool continueOnCapturedContext)
        {
            _value = value;
            _continueOnCapturedContext = continueOnCapturedContext;
        }

        /// <summary>Returns an awaiter for this <see cref="ConfiguredValueTaskAwaitable{TResult}"/> instance.</summary>
        public ConfiguredValueTaskAwaiter GetAwaiter()
        {
            return new ConfiguredValueTaskAwaiter(_value, _continueOnCapturedContext);
        }

        /// <summary>Provides an awaiter for a <see cref="ConfiguredValueTaskAwaitable{TResult}"/>.</summary>
        [StructLayout(LayoutKind.Auto)]
        public struct ConfiguredValueTaskAwaiter : ICriticalNotifyCompletion
        {
            /// <summary>The value being awaited.</summary>
            private readonly ValueTask _value;
            /// <summary>The value to pass to ConfigureAwait.</summary>
            private readonly bool _continueOnCapturedContext;

            /// <summary>Initializes the awaiter.</summary>
            /// <param name="value">The value to be awaited.</param>
            /// <param name="continueOnCapturedContext">The value to pass to ConfigureAwait.</param>
            internal ConfiguredValueTaskAwaiter(ValueTask value, bool continueOnCapturedContext)
            {
                _value = value;
                _continueOnCapturedContext = continueOnCapturedContext;
            }

            /// <summary>Gets whether the <see cref="ConfiguredValueTaskAwaitable{TResult}"/> has completed.</summary>
            public bool IsCompleted { get { return _value.IsCompleted; } }

            /// <summary>Gets the result of the ValueTask.</summary>
            public void GetResult()
            {
                if (_value._task != null)
                    _value._task.GetAwaiter().GetResult();
            }

            /// <summary>Schedules the continuation action for the <see cref="ConfiguredValueTaskAwaitable{TResult}"/>.</summary>
            public void OnCompleted(Action continuation)
            {
                _value.AsTask().ConfigureAwait(_continueOnCapturedContext).GetAwaiter().OnCompleted(continuation);
            }

            /// <summary>Schedules the continuation action for the <see cref="ConfiguredValueTaskAwaitable{TResult}"/>.</summary>
            public void UnsafeOnCompleted(Action continuation)
            {
                _value.AsTask().ConfigureAwait(_continueOnCapturedContext).GetAwaiter().UnsafeOnCompleted(continuation);
            }
        }
    }

    /// <summary>Provides an awaiter for a <see cref="ValueTask{TResult}"/>.</summary>
    public struct ValueTaskAwaiter : ICriticalNotifyCompletion
    {
        /// <summary>The value being awaited.</summary>
        private readonly ValueTask _value;

        /// <summary>Initializes the awaiter.</summary>
        /// <param name="value">The value to be awaited.</param>
        internal ValueTaskAwaiter(ValueTask value) { _value = value; }

        /// <summary>Gets whether the <see cref="ValueTask{TResult}"/> has completed.</summary>
        public bool IsCompleted { get { return _value.IsCompleted; } }

        /// <summary>Gets the result of the ValueTask.</summary>
        public void GetResult()
        {
            if (_value._task != null)
                _value._task.GetAwaiter().GetResult();
        }

        /// <summary>Schedules the continuation action for this ValueTask.</summary>
        public void OnCompleted(Action continuation)
        {
            _value.AsTask().ConfigureAwait(continueOnCapturedContext: true).GetAwaiter().OnCompleted(continuation);
        }

        /// <summary>Schedules the continuation action for this ValueTask.</summary>
        public void UnsafeOnCompleted(Action continuation)
        {
            _value.AsTask().ConfigureAwait(continueOnCapturedContext: true).GetAwaiter().UnsafeOnCompleted(continuation);
        }
    }


    class IdSeed
    {
        private static int _idSeed;
        public static int NewId()
        {
            return Interlocked.Increment(ref _idSeed);
        }
    }

    class AsyncValueTaskMethodBuilderState
    {
        public int _state;
    }

    /// <summary>Represents a builder for asynchronous methods that returns a <see cref="ValueTask{TResult}"/>.</summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    [StructLayout(LayoutKind.Auto)]
    public struct AsyncValueTaskMethodBuilder
    {
        private int _id;
        /// <summary>The <see cref="AsyncTaskMethodBuilder{TResult}"/> to which most operations are delegated.</summary>
        private AsyncTaskMethodBuilder _methodBuilder;
        /// <summary>The result for this builder, if it's completed before any awaits occur.</summary>
        /// <summary>true if <see cref="_result"/> contains the synchronous result for the async method; otherwise, false.</summary>
        //private bool _haveResult;
        /// <summary>true if the builder should be used for setting/getting the result; otherwise, false.</summary>
        //private bool _useBuilder;
        private int _state;
        private AsyncValueTaskMethodBuilderState _astate;

        private const int USE_BUIDER = 1;
        private const int HAS_RESULT = 2;

        /// <summary>Creates an instance of the <see cref="AsyncValueTaskMethodBuilder{TResult}"/> struct.</summary>
        /// <returns>The initialized instance.</returns>
        public static AsyncValueTaskMethodBuilder Create() => new AsyncValueTaskMethodBuilder()
        {
            _id = IdSeed.NewId(),
            _astate = new AsyncValueTaskMethodBuilderState(),
            _methodBuilder = AsyncTaskMethodBuilder.Create()
        };

        /// <summary>Begins running the builder with the associated state machine.</summary>
        /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine instance, passed by reference.</param>
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            _methodBuilder.Start(ref stateMachine); // will provide the right ExecutionContext semantics
        }

        /// <summary>Associates the builder with the specified state machine.</summary>
        /// <param name="stateMachine">The state machine instance to associate with the builder.</param>
        public void SetStateMachine(IAsyncStateMachine stateMachine) => _methodBuilder.SetStateMachine(stateMachine);

        /// <summary>Marks the task as successfully completed.</summary>
        /// <param name="result">The result to use to complete the task.</param>
        public void SetResult()
        {
            //Trace.TraceInformation($"AsyncValueTaskMethodBuilder[{_id}]: SetResult()");

            if (_id == 7)
            {
                //Trace.TraceInformation($"AsyncValueTaskMethodBuilder[{_id}]: SetResult() for id 7");
            }

            var state = Interlocked.CompareExchange(ref _astate._state, HAS_RESULT, 0);

            if (/*_useBuilder || useBulder ||*/ state == USE_BUIDER)
            {
                //Trace.TraceInformation($"AsyncValueTaskMethodBuilder[{_id}]: SetResult() - state=USE_BUIDER");
                _methodBuilder.SetResult();
            }
            else
            {
                //Trace.TraceInformation($"AsyncValueTaskMethodBuilder[{_id}]: SetResult() - state=HAS_RESULT");
                //_haveResult = true;
                
            }
        }

        /// <summary>Marks the task as failed and binds the specified exception to the task.</summary>
        /// <param name="exception">The exception to bind to the task.</param>
        public void SetException(Exception exception) => _methodBuilder.SetException(exception);

        /// <summary>Gets the task for this builder.</summary>
        public ValueTask Task
        {
            get
            {
                //Trace.TraceInformation($"AsyncValueTaskMethodBuilder[{_id}]: Task");

                //if (_id == 7)
                //{
                //    Trace.TraceInformation($"AsyncValueTaskMethodBuilder[{_id}]: Task for id 7");
                //}

                var state = Interlocked.CompareExchange(ref _astate._state, USE_BUIDER, 0);
                

                if (/*_haveResult || hasResult ||*/ state == HAS_RESULT)
                {
                    //Trace.TraceInformation($"AsyncValueTaskMethodBuilder[{_id}]: Task - state=HAS_RESULT");
                    return new ValueTask();
                }
                else
                {
                    //_useBuilder = true;
                    //Trace.TraceInformation($"AsyncValueTaskMethodBuilder[{_id}]: Task - state=USE_BUIDER");
                    return new ValueTask(_methodBuilder.Task);
                }
            }
        }

        /// <summary>Schedules the state machine to proceed to the next action when the specified awaiter completes.</summary>
        /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
        /// <param name="awaiter">the awaiter</param>
        /// <param name="stateMachine">The state machine.</param>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            _methodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);
        }

        /// <summary>Schedules the state machine to proceed to the next action when the specified awaiter completes.</summary>
        /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
        /// <param name="awaiter">the awaiter</param>
        /// <param name="stateMachine">The state machine.</param>
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            _methodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
        }
    }
}

namespace System.Threading.Tasks
{
    [AsyncMethodBuilder(typeof(AsyncValueTaskMethodBuilder))]
    [StructLayout(LayoutKind.Auto)]
    public struct ValueTask : IEquatable<ValueTask>
    {
        /// <summary>The task to be used if the operation completed asynchronously or if it completed synchronously but non-successfully.</summary>
        internal readonly Task _task;
        /// <summary>The result to be used if the operation completed successfully synchronously.</summary>

        /// <summary>
        /// Initialize the <see cref="ValueTask{TResult}"/> with a <see cref="Task{TResult}"/> that represents the operation.
        /// </summary>
        /// <param name="task">The task.</param>
        public ValueTask(Task task)
        {
            _task = task ?? throw new ArgumentNullException(nameof(task));
        }

        /// <summary>Returns the hash code for this instance.</summary>
        public override int GetHashCode()
        {
            return _task != null ? _task.GetHashCode() : 0;
        }

        /// <summary>Returns a value indicating whether this value is equal to a specified <see cref="object"/>.</summary>
        public override bool Equals(object obj)
        {
            return obj is ValueTask && Equals((ValueTask)obj);
        }

        /// <summary>Returns a value indicating whether this value is equal to a specified <see cref="ValueTask{TResult}"/> value.</summary>
        public bool Equals(ValueTask other)
        {
            return _task != null || other._task != null ? _task == other._task : false;
        }

        /// <summary>Returns a value indicating whether two <see cref="ValueTask{TResult}"/> values are equal.</summary>
        public static bool operator ==(ValueTask left, ValueTask right)
        {
            return left.Equals(right);
        }

        /// <summary>Returns a value indicating whether two <see cref="ValueTask{TResult}"/> values are not equal.</summary>
        public static bool operator !=(ValueTask left, ValueTask right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Gets a <see cref="Task{TResult}"/> object to represent this ValueTask.  It will
        /// either return the wrapped task object if one exists, or it'll manufacture a new
        /// task object to represent the result.
        /// </summary>
        public Task AsTask()
        {
            // Return the task if we were constructed from one, otherwise manufacture one.  We don't
            // cache the generated task into _task as it would end up changing both equality comparison
            // and the hash code we generate in GetHashCode.
            return _task ?? Task.FromResult(true);
        }

        /// <summary>Gets whether the <see cref="ValueTask{TResult}"/> represents a completed operation.</summary>
        public bool IsCompleted { get { return _task == null || _task.IsCompleted; } }

        /// <summary>Gets whether the <see cref="ValueTask{TResult}"/> represents a successfully completed operation.</summary>
        public bool IsCompletedSuccessfully { get { return _task == null || _task.Status == TaskStatus.RanToCompletion; } }

        /// <summary>Gets whether the <see cref="ValueTask{TResult}"/> represents a failed operation.</summary>
        public bool IsFaulted { get { return _task != null && _task.IsFaulted; } }

        /// <summary>Gets whether the <see cref="ValueTask{TResult}"/> represents a canceled operation.</summary>
        public bool IsCanceled { get { return _task != null && _task.IsCanceled; } }

        /// <summary>Gets the result.</summary>
        //public object Result { get { return _task == null ? _result : _task.GetAwaiter().GetResult(); } }

        /// <summary>Gets an awaiter for this value.</summary>
        public ValueTaskAwaiter GetAwaiter()
        {
            return new ValueTaskAwaiter(this);
        }

        /// <summary>Configures an awaiter for this value.</summary>
        /// <param name="continueOnCapturedContext">
        /// true to attempt to marshal the continuation back to the captured context; otherwise, false.
        /// </param>
        public ConfiguredValueTaskAwaitable ConfigureAwait(bool continueOnCapturedContext)
        {
            return new ConfiguredValueTaskAwaitable(this, continueOnCapturedContext: continueOnCapturedContext);
        }

        /// <summary>Gets a string-representation of this <see cref="ValueTask{TResult}"/>.</summary>
        //public override string ToString()
        //{
        //    if (_task != null)
        //    {
        //        return _task.Status == TaskStatus.RanToCompletion ?
        //            _task.Result.ToString() :
        //            string.Empty;
        //    }
        //    else
        //    {
        //        return _result != null ?
        //            _result.ToString() :
        //            string.Empty;
        //    }
        //}

        // TODO: Remove CreateAsyncMethodBuilder once the C# compiler relies on the AsyncBuilder attribute.

        /// <summary>Creates a method builder for use with an async method.</summary>
        /// <returns>The created builder.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)] // intended only for compiler consumption
        public static AsyncValueTaskMethodBuilder CreateAsyncMethodBuilder() => AsyncValueTaskMethodBuilder.Create();
    }
}