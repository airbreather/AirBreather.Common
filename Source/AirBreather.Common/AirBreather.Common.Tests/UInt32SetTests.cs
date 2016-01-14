using System.Collections.Generic;
using System.Diagnostics;

using Xunit;
using Xunit.Abstractions;

using AirBreather.Common.Collections;
using AirBreather.Common.Random;

namespace AirBreather.Common.Tests
{
    public sealed class UInt32SetTests
    {
        private readonly ITestOutputHelper output;

        public UInt32SetTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test()
        {
            const int ValueCount = 900000;
            const int TestCount = 20000000;
            var vals = new uint[ValueCount];
            var testVals = new uint[TestCount];

            // a RNG used to generate truly random seeds for (faster) PRNGs
            var initRand = new CryptographicRandomGenerator();

            var rand = new MT19937_64Generator();
            var state = new MT19937_64State(initRand.NextUInt64());
            state = rand.FillBuffer(state, vals);
            state = rand.FillBuffer(state, testVals);

            UInt32Set mySet = new UInt32Set(vals);
            HashSet<uint> realSet = new HashSet<uint>(vals);

            byte[] myResults = new byte[TestCount];
            byte[] realResults = new byte[TestCount];

            // run mine first so it's the one that's disadvantaged if there's a
            // caching-related bias that unfairly benefits one of the two.
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < testVals.Length; i++)
            {
                myResults[i] = mySet.Contains(testVals[i]) ? (byte)1 : (byte)0;
            }

            sw.Stop();

            long mySetTicks = sw.ElapsedTicks;

            sw.Restart();
            for (int i = 0; i < testVals.Length; i++)
            {
                realResults[i] = realSet.Contains(testVals[i]) ? (byte)1 : (byte)0;
            }

            sw.Stop();

            this.output.WriteLine("{0} seconds for built-in HashSet<uint>", sw.ElapsedTicks / (double)Stopwatch.Frequency);
            this.output.WriteLine("{0} seconds for my UInt32Set", mySetTicks / (double)Stopwatch.Frequency);
            this.output.WriteLine("Mine took {0:P2} as long to run as the built-in did", mySetTicks / (double)sw.ElapsedTicks);

            Assert.Equal(realResults, myResults, FastByteArrayEqualityComparer.Instance);
        }
    }
}
