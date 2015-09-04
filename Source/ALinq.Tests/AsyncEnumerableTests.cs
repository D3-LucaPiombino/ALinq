using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace ALinq.Tests
{
    [TestClass]
    public class AsyncEnumerableTests
    {
        [TestMethod]
        public async Task EnumerationShouldBeLazy()
        {
            var sequence = AsyncEnumerable.Create<int>(async p =>
            {
                await p.Yield(1);
                await p.Yield(2);
                await p.Yield(3);
                throw new Exception("This should never have been thrown!");
            });
            await sequence.Any(i => i == 2);
        }

        [TestMethod]
        public async Task ItShouldBePossibleToImplementInfiniteSequences()
        {
            var sequence = AsyncEnumerable.Create<int>(async p =>
            {
                int i = 0;
                while(true)
                {
                    await p.Yield(i++);
                }
            });

            var list = await sequence.Take(10).ToList();
            CollectionAssert.AreEqual(Enumerable.Range(0,10).ToList(), (ICollection)list);
        }


        [TestMethod]
        public async Task ItShouldBePossibileToAbandonAnEnumerationWithoutLeaking()
        {
            var outerReference = new Uri("http://tempuri.org");

            //var enumerable = EnumerableStateMachine(outerReference);
            //enumerable.Take(10).ToList();



            var probeWeakReference = new WeakReference<Uri>(outerReference);
            var sequence = AsyncEnumerable.Create<string>(async p =>
            {
                // This will be hoisted in the state machine that is generated 
                // by the compiler.
                Uri hoistedReference;
                Assert.IsTrue(probeWeakReference.TryGetTarget(out hoistedReference));
                int i = 0;
                while (true)
                {
                    await // This will register a callback that reference 
                          // this state machine on the threadpool, that in 
                          // turn will keep alive the instance referenced 
                          // by `hoistedReference`
                        p.Yield(hoistedReference.ToString() + i++);
                }
            });

            // Just a sanity check to verify that the state machine has been created and did what we expected.
            var expectedList = Enumerable.Range(0, 10).Select(i => outerReference.ToString() + i).ToList();
            var asyncList = await sequence.Take(10).ToList();
            CollectionAssert.AreEqual(expectedList, (ICollection)asyncList);


            // Release our reference to the Uri.
            // If the continuation that point to the next step in the asynchronous state machine has not 
            // been removed from the thread pool queue, weakReference.Target  will not be null after the 
            // GC run.
            outerReference = null;

            GC.Collect(2, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();

            Uri unused;
            Assert.IsFalse(
                probeWeakReference.TryGetTarget(out unused),
                "The reference to the Uri should have been collected.\r\n" +
                "This probably means that the state machine has not been properly disposed and " +
                "is still keep alive by one or more references in the thread pool."
            );
        }


        [TestMethod]
        public async Task ShouldThrowIfYieldIsCalledInAFinallyClauseAndTheEnumeratorHasBeenDisposed()
        {
            
            var sequence = AsyncEnumerable.Create<int>(async p =>
            {
                try
                {
                    while (true)
                    {
                        try
                        {
                            await p.Yield(1);
                        }
                        catch (Exception) when(!p.EnumerationCancelled)
                        {
                            await p.Yield(2);
                        }
                        
                    }
                }
                finally
                {
                    await p.Yield(3);
                }
            });
            try
            {
                await sequence.Take(1).ToList();
                Assert.Fail("An ObjectDisposedException should have been thrown.");
            }
            catch (YieldCalledAfterEnumerationHasBeenAbandonedException)
            {

            }
        }

        [TestMethod]
        public async Task ToEnumerableShouldBeUsableWithForeach()
        {
            var sequence = AsyncEnumerable.Create<int>(async p =>
            {
                await p.Yield(1);
                await p.Yield(2);
                await p.Yield(3);
                throw new Exception("This should never have been thrown!");
            });
            foreach (var task in sequence.ToEnumerable())
            {
                var item = await task;
                if (!item.HasValue)
                    break;

                if (item.Value > 2)
                    break;
            }
        }






        private IEnumerable<string> EnumerableStateMachine(Uri hoistedReference)
        {
            try
            {
                var i = 0;
                while (true)
                {
                    yield return hoistedReference.ToString() + i++;
                }
            }
            finally
            {
                
            }
        }
    }
}