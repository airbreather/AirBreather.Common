using System.Linq;
using System.Threading.Tasks;

using Xunit;

using AirBreather.Common.Random;
using AirBreather.Common.Utilities;

namespace AirBreather.Common.Tests
{
    public sealed class LazyBufferedEnumerableTests
    {
        [Fact]
        public void SimpleTest()
        {
            var rng0 = new CryptographicRandomGenerator();

            // this sequence will throw if iterated over multiple times.
            ulong?[] vals = new ulong?[4096];
            var seq = Enumerable.Range(0, vals.Length)
                                .Select(i =>
                                {
                                    Assert.False(vals[i].HasValue);
                                    return (ulong)(vals[i] = rng0.NextUInt64());
                                });

            using (var lazyBufferedSeq = new LazyBufferedEnumerable<ulong>(seq))
            {
                ulong[] vals2 = new ulong[vals.Length];

                // spawn tons of concurrent tasks, all but one of which stops before the end.
                Parallel.For(0, vals2.Length, i =>
                {
                    int j = 0;
                    foreach (var val in lazyBufferedSeq.Take(i + 1))
                    {
                        vals2[j++] = val;
                    }
                });

                Assert.All(vals, val => Assert.True(val.HasValue));
                Assert.Equal(vals.Select(val => val.Value), vals2);
            }
        }
    }
}
