using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ALinq.Tests
{
    internal static class Ext
    {
        public static Task<T> AsTask<T>(this T item)
        {
            return Task.Run(() => item);
        }
    }
    [TestClass]
    public class AsyncEnumerableSelectManyTests
    {
        
        public class Entry
        {
            private readonly int start;

            public IAsyncEnumerable<int> Member
            {
                get { return AsyncEnumerable.Range(start, 10); }
            }

            public Entry(int start)
            {
                this.start = start;
            }
        }

        [TestMethod]
        public async Task SelectManyShouldWork()
        {
            var sequence = await AsyncEnumerable.Range(0, 10)
                .Select(i => new Entry(i * 10).AsTask())
                .SelectMany((entry,index) => entry.Member.AsTask(), (entry, item) => item.AsTask())
                .ToList();
            
            CollectionAssert.AreEqual(Enumerable.Range(0,100).ToList(),sequence.ToList());
        }



        [TestMethod]
        public async Task SelectManyShouldWork2()
        {
            var sequence = await AsyncEnumerable.Range(0, 10)
                .Select(i => new Entry(i * 10).AsTask())
                .SelectMany((entry, index) => entry.Member, (entry, item) => item.AsTask())
                .ToList();

            CollectionAssert.AreEqual(Enumerable.Range(0, 100).ToList(), sequence.ToList());
        }


        public async Task SelectManyShouldWork3()
        {
            var sequence = await AsyncEnumerable.Range(0, 10)
                .Select(i => new Entry(i * 10).AsTask())
                .SelectMany((entry, index) => entry.Member.AsTask(), (entry, item) => item)
                .ToList();

            CollectionAssert.AreEqual(Enumerable.Range(0, 100).Select(i => i*2).ToList(), sequence.ToList());
        }


        public async Task SelectManyShouldWork4()
        {
            var sequence = await AsyncEnumerable.Range(0, 10)
                .Select(i => new Entry(i * 10).AsTask())
                .SelectMany((entry, index) => entry.Member, (entry, item) => item)
                .ToList();

            CollectionAssert.AreEqual(Enumerable.Range(0, 100).Select(i => i * 2).ToList(), sequence.ToList());
        }
    }
}