namespace ALinq
{
    public static partial class AsyncEnumerable
    {
        public static IAsyncEnumerable<T> Empty<T>()
        {
            return EmptyAsyncEnumerable<T>.Instance;
        }
    }
}