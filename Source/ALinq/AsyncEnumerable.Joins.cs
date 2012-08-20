﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> enumerable,IAsyncEnumerable<T> otherEnumerable)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (otherEnumerable == null) throw new ArgumentNullException("otherEnumerable");

            return Create<T>(async producer =>
            {
                await enumerable.ForEach(async item => await producer.Yield(item));
                await otherEnumerable.ForEach(async item => await producer.Yield(item));
            });
        }

        public static IAsyncEnumerable<TResult> Zip<TFirst,TSecond,TResult>(this IAsyncEnumerable<TFirst> enumerable1,IAsyncEnumerable<TSecond> enumerable2,Func<TFirst,TSecond,Task<TResult>> merger)
        {
            if (enumerable1 == null) throw new ArgumentNullException("enumerable1");
            if (enumerable2 == null) throw new ArgumentNullException("enumerable2");
            if (merger == null) throw new ArgumentNullException("merger");

            return Create<TResult>(async producer =>
            {
                var enumerator1 = enumerable1.GetEnumerator();
                var enumerator2 = enumerable2.GetEnumerator();
                var doContinue  = true;

                try
                {
                    while( doContinue )
                    {
                        if ( await enumerator1.MoveNext() && await enumerator2.MoveNext())
                        {
                            await producer.Yield(await merger(enumerator1.Current, enumerator2.Current));
                        }

                        doContinue = false;
                    }
                }
                finally
                {
                    enumerator1.Dispose();
                    enumerator2.Dispose();
                }
            });
        }

        public static async Task<bool> SequenceEqual<T>(this IAsyncEnumerable<T> enumerable1,IAsyncEnumerable<T> enumerable2)
        {
            return await SequenceEqual<T>(enumerable1, enumerable2, EqualityComparer<T>.Default);
        }

        public static async Task<bool> SequenceEqual<T>(this IAsyncEnumerable<T> enumerable1,IAsyncEnumerable<T> enumerable2,IEqualityComparer<T> comparer)
        {
            if (enumerable1 == null) throw new ArgumentNullException("enumerable1");
            if (enumerable2 == null) throw new ArgumentNullException("enumerable2");

            if (comparer == null) throw new ArgumentNullException("comparer");

            var enumerator1 = enumerable1.GetEnumerator();
            var enumerator2 = enumerable2.GetEnumerator();

            try
            {
                while (true)
                {
                    var move1 = await enumerator1.MoveNext();
                    var move2 = await enumerator2.MoveNext();

                    if (move1 && move2)
                    {
                        if ( !comparer.Equals(enumerator1.Current,enumerator2.Current))
                        {
                            return false;
                        }
                    }
                    else if ( move1 || move2 )
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            finally
            {
                enumerator1.Dispose();
                enumerator2.Dispose();
            }
        }
    }
}