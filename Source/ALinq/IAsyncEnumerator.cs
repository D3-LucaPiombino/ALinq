using System;
using System.Threading.Tasks;

namespace ALinq
{
    public interface IAsyncEnumerator
    {
        object      Current { get; }
        Task<bool>  MoveNext();

        // Task<bool> MoveNext(CancellationToken cancellationToken, Exception e);
    }

    public interface IAsyncEnumerator<out T> : IAsyncEnumerator
    {
        new T Current { get; }
    }
}