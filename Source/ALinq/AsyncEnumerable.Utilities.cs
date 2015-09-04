using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static void Dispose(this IAsyncEnumerator asyncEnumerator)
        {
            var disposable = asyncEnumerator as IDisposable;
            disposable?.Dispose();
        }

        public static Task DisposeAsync(this IAsyncEnumerator enumerator)
        {
            var asyncDisposable = enumerator as IAsyncDisposable;
            if (asyncDisposable != null)
            {
                return asyncDisposable.DisposeAsync();
            }
            enumerator.Dispose();
            return Task.FromResult(true);
        }

        /// <summary>
        /// Ensure that the <paramref name="enumerator"/> is disposed correctly and if an exception is thrown and 
        /// <paramref name="outerException"/> is not null, throw an <see cref="AggregateException"/>
        /// that aggregate both the exception thrown by the enumerator and the exception containted in 
        /// <paramref name="outerException"/>
        /// </summary>
        /// <param name="enumerator"></param>
        /// <param name="outerException"></param>
        /// <param name="otherEnumerator"></param>
        /// <returns></returns>
        /// <throw ><see cref="AggregateException"/></throw>
        public static async Task DisposeAsync(this IAsyncEnumerator enumerator, Exception outerException)
        {
            try
            {
                await enumerator.DisposeAsync();
            }
            catch (Exception ex)
            {
                if (outerException != null)
                    throw new AggregateException(outerException, ex);
                throw;
            }
        }

        /// <summary>
        /// Ensure that both <paramref name="enumerator"/> and <paramref name="otherEnumerator"/> are disposed
        /// correctly and if an exception is thrown and <paramref name="outerException"/> is not null, throw an <see cref="AggregateException"/>
        /// that aggregate both the exception thrown by the enumerator/s and the exception containted in <paramref name="outerException"/>
        /// </summary>
        /// <param name="enumerator"></param>
        /// <param name="outerException"></param>
        /// <param name="otherEnumerator"></param>
        /// <returns></returns>
        /// <throw ><see cref="AggregateException"/></throw>
        public static async Task DisposeAsync(this IAsyncEnumerator enumerator, Exception outerException, IAsyncEnumerator otherEnumerator)
        {
            try
            {
                await Task.WhenAll(enumerator.DisposeAsync(), otherEnumerator.DisposeAsync());
            }
            catch (Exception ex)
            {
                if (outerException != null)
                    throw new AggregateException(outerException, ex);
                throw;
            }
        }


        /// <summary>
        /// Wait for a task to complete sinchronously and if an <see cref="AggregateException"/> is 
        /// thrown that contains only a single exception and rethrow the inner exception.
        /// </summary>
        /// <param name="task"></param>
        internal static void WaitAndUnwrapAggregateException(this Task task)
        {
            try
            {
                task?.Wait();
            }
            catch (AggregateException ae) when(ae.InnerExceptions.Count == 1)
            {
                ExceptionDispatchInfo.Capture(ae.InnerException).Throw();
                throw;
            }
        }

        internal static bool CanBeDisposed(this Task task)
        {
            return task.IsCanceled || task.IsCompleted || task.IsFaulted;
        }

        internal static bool TryDispose(this Task task)
        {
            if (!task.CanBeDisposed())
                return false;
            task.Dispose();
            return true;
        }
    }
}