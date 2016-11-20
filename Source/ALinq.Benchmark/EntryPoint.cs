using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ALinq.Benchmark
{
    public static class EntryPoint
    {
        public class Profiler : IDisposable
        {
            private readonly string     name;
            private readonly Stopwatch  stopWatch = new Stopwatch();

            public void Dispose()
            {
                stopWatch.Stop();
                Console.WriteLine("{0}: {1}",name,stopWatch.Elapsed);
            }

            public Profiler(string name)
            {
                this.name = name;
                stopWatch.Start();
            }
        }

        public class SortEntry : IEquatable<SortEntry>
        {
            public int Id1 { get; private set; }
            public int Id2 { get; private set; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((SortEntry)obj);
            }

            public bool Equals(SortEntry other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Id1 == other.Id1 && Id2 == other.Id2;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Id1 * 397) ^ Id2;
                }
            }

            public SortEntry(int id1, int id2)
            {
                this.Id1 = id1;
                this.Id2 = id2;
            }
        }

        public static void Main(string[] arguments) => AsyncMain(arguments).GetAwaiter().GetResult();

        public static async ValueTask AsyncMain(string[] arguments)
        {
            Console.ReadKey();
            //Scenario01(100 * 100).Wait();
            //Scenario02(1000 * 1000);

            //for (int i = 0; i < 10; i++)
            //    Scenario04(10000);
            //for (int i = 0; i < 10; i++)
                await Scenario03(10000);
            //Scenario05(1000).Wait();
            //Scenario06(100).Wait();

            Console.ReadKey();
        }
        private static async Task Scenario01(int size)
        {
            var random      = new Random();
            var data        = Enumerable.Range(0, size).Select(i => new SortEntry(random.Next(0, 100), random.Next(0, 100))).ToList();

            using (new Profiler("ALINQ Scenario01"))
            {
                var result =
                    await
                    data.ToAsync().OrderBy(async i => i.Id1).ThenBy(async i => i.Id2).ToList();
            }

        }

        private static void Scenario02(int size)
        {
            var random = new Random();
            var data = Enumerable.Range(0, size).Select(i => new SortEntry(random.Next(0, 100), random.Next(0, 100))).ToList();

            using (new Profiler("LINQ Scenario02"))
            {
                var orderedData = data.OrderBy(d => d.Id1).ThenBy(d => d.Id2).ToList();
            }
        }

        private static async ValueTask Scenario03(int size)
        {
            using (new Profiler("ALINQ Scenario03"))
            {
                //var data = await AsyncEnumerable.Range(0, size).ToList();
                var data = await AsyncGenerator(size)
                    //.Select(async item => item * 2)
                    //.Select(async item => item * 2)
                    //.Select(async item => item * 2)
                    //.Select(async item => item * 2)
                    //.Select(async item => item * 2)
                    .ToList();
            }

        }

        public static IEnumerable<int> Generator(int size)
        {
            for (int i = 0; i < size; i++)
            {
                yield return i;
                
            }
        }

        public static IAsyncEnumerable<int> AsyncGenerator(int size)
        {
            return AsyncEnumerable.Create<int>(async p =>
            {
                for (int i = 0; i < size; i++)
                {
                    //Trace.TraceInformation($"Yielding {i}");
                    //Thread.Sleep(10);
                    await p.Yield(i);
                    if ((i % 1000) == 0) Console.WriteLine(i);
                    //Trace.TraceInformation($"Yielded {i}");
                }
                
            });
        }

        private static void Scenario04(int size)
        {
            using (new Profiler("LINQ Scenario04"))
            {
                Generator(size).ToList();
            }
        }


        private static async Task Scenario05(int size)
        {
            var a = AsyncEnumerable.Create<int>(async producer =>
            {
                for (int i = 0; i < size; i++)
                {
                    await producer.Yield(1);
                }
            });
            var list = await a.ToList();
        }


        private static async Task Scenario06(int size)
        {
            var outer = AsyncEnumerable.Create<IAsyncEnumerable<int>>(async producer =>
            {
                for (int i = 0; i < size; i++)
                {
                    var inner = AsyncEnumerable.Create<int>(async innerProducer =>
                    {
                        for (int j = 0; j < size/10; j++)
                        {
                            await innerProducer.Yield(j);
                        }
                    });
                    await producer.Yield(inner);
                }
            });

            

            var list = await outer
                .SelectMany(inner => inner.Select(i => i * 100))
                .ToList();
        }


    }
}
