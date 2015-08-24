﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ALinq.Tests
{
    [TestClass]
    public class AsyncEnumerableMaxTests
    {
        [TestMethod]
        public async Task MaxShouldWork()
        {
            Assert.AreEqual(9, await GetRange(5, 5, i => i).Max());
            Assert.AreEqual(9L, await GetRange(5, 5, i => (long)i).Max());
            Assert.AreEqual(9.0f, await GetRange(5, 5, i => (float)i).Max());
            Assert.AreEqual(9.0, await GetRange(5, 5, i => (double)i).Max());
            Assert.AreEqual(9.0m, await GetRange(5, 5, i => (decimal)i).Max());

            Assert.AreEqual(9, await GetRange(5, 5, i => (int?)i).Max());
            Assert.AreEqual(9L, await GetRange(5, 5, i => (long?)i).Max());
            Assert.AreEqual(9.0f, await GetRange(5, 5, i => (float?)i).Max());
            Assert.AreEqual(9.0, await GetRange(5, 5, i => (double?)i).Max());
            Assert.AreEqual(9.0m, await GetRange(5, 5, i => (decimal?)i).Max());
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            Assert.AreEqual(9, await GetRange(5, 5, i => i).Select(async i => Tuple.Create(i)).Max(async i => i.Item1));
            Assert.AreEqual(9L, await GetRange(5, 5, i => (long)i).Select(async i => Tuple.Create(i)).Max(async i => i.Item1));
            Assert.AreEqual(9.0f, await GetRange(5, 5, i => (float)i).Select(async i => Tuple.Create(i)).Max(async i => i.Item1));
            Assert.AreEqual(9.0, await GetRange(5, 5, i => (double)i).Select(async i => Tuple.Create(i)).Max(async i => i.Item1));
            Assert.AreEqual(9.0m, await GetRange(5, 5, i => (decimal)i).Select(async i => Tuple.Create(i)).Max(async i => i.Item1));

            Assert.AreEqual(9, await GetRange(5, 5, i => (int?)i).Select(async i => Tuple.Create(i)).Max(async i => i.Item1));
            Assert.AreEqual(9L, await GetRange(5, 5, i => (long?)i).Select(async i => Tuple.Create(i)).Max(async i => i.Item1));
            Assert.AreEqual(9.0f, await GetRange(5, 5, i => (float?)i).Select(async i => Tuple.Create(i)).Max(async i => i.Item1));
            Assert.AreEqual(9.0, await GetRange(5, 5, i => (double?)i).Select(async i => Tuple.Create(i)).Max(async i => i.Item1));
            Assert.AreEqual(9.0m, await GetRange(5, 5, i => (decimal?)i).Select(async i => Tuple.Create(i)).Max(async i => i.Item1));
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        }

        private static IAsyncEnumerable<T> GetRange<T>(int start, int count, Func<int, T> converter)
        {
            return Enumerable.Range(start, count).Select(converter).ToList().ToAsync();
        }
    }
}
