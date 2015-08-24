﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ALinq.Tests
{
    [TestClass]
    public class AsyncEnumerableSumTests
    {
        [TestMethod]
        public async Task SumShouldWork()
        {
            Assert.AreEqual(45,await GetRange(0,10,i => i).Sum());
            Assert.AreEqual(45L,await GetRange(0, 10, i => (long)i).Sum());
            Assert.AreEqual(45.0f,await GetRange(0, 10, i => (float)i).Sum());
            Assert.AreEqual(45.0,await GetRange(0, 10, i => (double)i).Sum());
            Assert.AreEqual(45.0m,await GetRange(0, 10, i => (decimal)i).Sum());

            Assert.AreEqual(45, await GetRange(0, 10, i => (int?)i).Sum());
            Assert.AreEqual(45L, await GetRange(0, 10, i => (long?)i).Sum());
            Assert.AreEqual(45.0f, await GetRange(0, 10, i => (float?)i).Sum());
            Assert.AreEqual(45.0, await GetRange(0, 10, i => (double?)i).Sum());
            Assert.AreEqual(45.0m, await GetRange(0, 10, i => (decimal?)i).Sum());
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            Assert.AreEqual(45, await GetRange(0, 10, i => i).Select(async i => Tuple.Create(i)).Sum(async i => i.Item1));
            Assert.AreEqual(45L, await GetRange(0, 10, i => (long)i).Select(async i => Tuple.Create(i)).Sum(async i => i.Item1));
            Assert.AreEqual(45.0f, await GetRange(0, 10, i => (float)i).Select(async i => Tuple.Create(i)).Sum(async i => i.Item1));
            Assert.AreEqual(45.0, await GetRange(0, 10, i => (double)i).Select(async i => Tuple.Create(i)).Sum(async i => i.Item1));
            Assert.AreEqual(45.0m, await GetRange(0, 10, i => (decimal)i).Select(async i => Tuple.Create(i)).Sum(async i => i.Item1));

            Assert.AreEqual(45, await GetRange(0, 10, i => (int?)i).Select(async i => Tuple.Create(i)).Sum(async i => i.Item1));
            Assert.AreEqual(45L, await GetRange(0, 10, i => (long?)i).Select(async i => Tuple.Create(i)).Sum(async i => i.Item1));
            Assert.AreEqual(45.0f, await GetRange(0, 10, i => (float?)i).Select(async i => Tuple.Create(i)).Sum(async i => i.Item1));
            Assert.AreEqual(45.0, await GetRange(0, 10, i => (double?)i).Select(async i => Tuple.Create(i)).Sum(async i => i.Item1));
            Assert.AreEqual(45.0m, await GetRange(0, 10, i => (decimal?)i).Select(async i => Tuple.Create(i)).Sum(async i => i.Item1));
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        }

        private static IAsyncEnumerable<T> GetRange<T>(int start,int count,Func<int,T> converter)
        {
            return Enumerable.Range(start, count).Select(converter).ToList().ToAsync();
        }
    }
}
