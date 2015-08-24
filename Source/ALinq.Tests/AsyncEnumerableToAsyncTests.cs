using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ALinq.Tests
{
    [TestClass]
    public class AsyncEnumerableToAsyncTests
    {
        // This cannot work!
        [TestMethod]
        public async Task ObservablesShouldBeConvertible()
        {
            //var observable = System.Reactive.Linq.Observable.Create<int>(async observer =>
            //{
            //    for( var i = 0; i < 10; ++i )
            //    {
            //        await Task.Delay(10).ConfigureAwait(false);
            //        observer.OnNext(i);
            //    }
            //});


            var observable = System.Reactive.Linq.Observable.Create<int>(async observer =>
            {
                for (var i = 0; i < 10; ++i)
                {
                    await Task.Delay(10).ConfigureAwait(false);
                    observer.OnNext(i);
                }

                await Task.Delay(10).ConfigureAwait(false);
            });

            var sequence            = observable.ToAsync();
            var evaluatedSequence   = await sequence.ToList();

            CollectionAssert.AreEqual(Enumerable.Range(0,10).ToList(),evaluatedSequence.ToList());
        }
    }
}