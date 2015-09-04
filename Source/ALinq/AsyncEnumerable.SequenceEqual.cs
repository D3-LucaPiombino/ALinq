using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static async Task<bool> SequenceEqual<T>(this IAsyncEnumerable<T> enumerable1, IAsyncEnumerable<T> enumerable2)
        {
            return await SequenceEqual<T>(enumerable1, enumerable2, EqualityComparer<T>.Default).ConfigureAwait(false);
        }

        public static async Task<bool> SequenceEqual<T>(this IAsyncEnumerable<T> enumerable1, IAsyncEnumerable<T> enumerable2, IEqualityComparer<T> comparer)
        {
            if (enumerable1 == null) throw new ArgumentNullException("enumerable1");
            if (enumerable2 == null) throw new ArgumentNullException("enumerable2");

            if (comparer == null) throw new ArgumentNullException("comparer");

            var enumerator1 = enumerable1.GetEnumerator();
            var enumerator2 = enumerable2.GetEnumerator();

            ExceptionDispatchInfo edi = null;
            try
            {
                while (true)
                {
                    var move1 = await enumerator1.MoveNext().ConfigureAwait(false);
                    var move2 = await enumerator2.MoveNext().ConfigureAwait(false);

                    if (move1 && move2)
                    {
                        if (!comparer.Equals(enumerator1.Current, enumerator2.Current))
                        {
                            return false;
                        }
                    }
                    else if (move1 || move2)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch(Exception e)
            {
                edi = ExceptionDispatchInfo.Capture(e);
            }
            finally
            {
                await enumerator1.DisposeAsync(edi?.SourceException, enumerator2);
            }

            edi?.Throw();
            return false;
        }
    }
}