using System;
using System.Collections.Generic;
using System.Linq;

namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<T> Reverse<T>(this IAsyncEnumerable<T> enumerable)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");

            return Create<T>(async producer =>
            {
                var evaluatedSequence = new List<T>();

                await enumerable.ForEach((item,_) => evaluatedSequence.Add(item)).ConfigureAwait(false);


                foreach (var item in ((IEnumerable<T>)evaluatedSequence).Reverse())
                {
                    await producer.Yield(item).ConfigureAwait(false);
                }
            });
        }
    }
}