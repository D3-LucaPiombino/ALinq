using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<TResult> Zip<TFirst,TSecond,TResult>(this IAsyncEnumerable<TFirst> enumerable1,IAsyncEnumerable<TSecond> enumerable2,Func<TFirst,TSecond,Task<TResult>> merger)
        {
            if (enumerable1 == null) throw new ArgumentNullException("enumerable1");
            if (enumerable2 == null) throw new ArgumentNullException("enumerable2");
            if (merger == null) throw new ArgumentNullException("merger");

            return Create<TResult>(async producer =>
            {
                var enumerator1 = enumerable1.GetEnumerator();
                var enumerator2 = enumerable2.GetEnumerator();

                ExceptionDispatchInfo edi = null;
                try
                {
                    while(await enumerator1.MoveNext().ConfigureAwait(false) && await enumerator2.MoveNext().ConfigureAwait(false))
                    {
                        var merged = await merger(enumerator1.Current, enumerator2.Current).ConfigureAwait(false);
                        await producer.Yield(merged).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    edi = ExceptionDispatchInfo.Capture(e);
                }
                finally
                {
                    await enumerator1.DisposeAsync(edi?.SourceException, enumerator2);
                }
                edi?.Throw();
            });
        }
    }
}