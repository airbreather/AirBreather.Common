using System;
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
            uint[] vals;
            uint[] testVals;

            var rand = new MT19937_64Generator();
            var state = new MT19937_64State(5489);

            {
                byte[] valData = new byte[ValueCount * 4];

                state = rand.FillBuffer(state, valData);
                vals = new uint[ValueCount];
                Buffer.BlockCopy(valData, 0, vals, 0, valData.Length);
            }

            {
                byte[] valData = new byte[ValueCount * 4];

                state = rand.FillBuffer(state, valData);
                testVals = new uint[TestCount];
                Buffer.BlockCopy(valData, 0, testVals, 0, valData.Length);
            }

            UInt32Set mySet = new UInt32Set(vals);
            HashSet<uint> realSet = new HashSet<uint>(vals);

            bool[] myResults = new bool[TestCount];
            bool[] realResults = new bool[TestCount];

            // run mine first so it's the one that's disadvantaged if there's a
            // caching-related bias that unfairly benefits one of the two.
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < testVals.Length; i++)
            {
                myResults[i] = mySet.Contains(testVals[i]);
            }

            sw.Stop();

            long mySetTicks = sw.ElapsedTicks;

            sw.Restart();
            for (int i = 0; i < testVals.Length; i++)
            {
                realResults[i] = realSet.Contains(testVals[i]);
            }

            sw.Stop();

            this.output.WriteLine("{0} seconds for built-in HashSet<uint>", sw.ElapsedTicks / (double)Stopwatch.Frequency);
            this.output.WriteLine("{0} seconds for my UInt32Set", mySetTicks / (double)Stopwatch.Frequency);
            this.output.WriteLine("Mine took {0:P2} as long to run as the built-in did", mySetTicks / (double)sw.ElapsedTicks);

            Assert.True(UnsafeCompare(realResults, myResults));
        }

        // based on http://stackoverflow.com/a/8808245/1083771
        static unsafe bool UnsafeCompare(bool[] a1, bool[] a2)
        {
            if (a1 == a2)
                return true;
            if (a1 == null || a2 == null || a1.Length != a2.Length)
                return false;
            fixed (bool * p1 = a1, p2 = a2)
            {
                bool* x1 = p1, x2 = p2;
                int l = a1.Length;
                for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                    if (*((long*)x1) != *((long*)x2)) return false;
                if ((l & 4) != 0) { if (*((int*)x1) != *((int*)x2)) return false; x1 += 4; x2 += 4; }
                if ((l & 2) != 0) { if (*((short*)x1) != *((short*)x2)) return false; x1 += 2; x2 += 2; }
                if ((l & 1) != 0) if (*x1 != *x2) return false;
                return true;
            }
        }
    }
}
