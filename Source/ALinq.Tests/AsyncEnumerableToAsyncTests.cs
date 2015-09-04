using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ALinq.Tests
{
    [TestClass]
    public class AsyncEnumerableToAsyncTests
    {
        
        [TestMethod]
        public async Task ObservablesShouldBeConvertible()
        {
            var observable = System.Reactive.Linq.Observable.Create<int>(observer =>
            {
                for (var i = 0; i < 10; ++i)
                {
                    observer.OnNext(i);
                }
                observer.OnCompleted();
                return () => { };
            });

            var sequence            = observable.ToAsync();
            var evaluatedSequence   = await sequence.ToList();

            CollectionAssert.AreEqual(Enumerable.Range(0,10).ToList(),evaluatedSequence.ToList());
        }
    }
}