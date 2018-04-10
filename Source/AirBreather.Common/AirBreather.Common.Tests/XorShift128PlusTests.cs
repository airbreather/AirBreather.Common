using System;
using System.Runtime.InteropServices;

using Xunit;

using AirBreather.Random;

namespace AirBreather.Tests
{
    public sealed class XorShift128PlusTests
    {
        [Theory]
        [InlineData(1234524356ul, 47845723665ul, 10356027574996968ul, 421627830503766283ul, 7267806761253193977ul)]
        [InlineData(262151541652562ul, 468594272265ul, 3923822141990852456ul, 3993942717521754294ul, 13070632098572223408ul)]
        public void Test(ulong s0, ulong s1, ulong expectedResult1, ulong expectedResult2, ulong expectedResult3)
        {
            // this was params ulong[], but I like InlineData too much to make that work...
            Span<ulong> expectedResults = stackalloc[] { expectedResult1, expectedResult2, expectedResult3 };

            var gen = new XorShift128PlusGenerator();
            var state = new RngState128(s0, s1);
            var buf = new byte[expectedResults.Length * 8].AsSpan();
            var actualResults = MemoryMarshal.Cast<byte, ulong>(buf);

            // First, do it in separate calls.
            for (int i = 0; i < expectedResults.Length; ++i)
            {
                state = gen.FillBuffer(state, buf.Slice(i * 8, 8));
                Assert.Equal(expectedResults[i], actualResults[i]);
            }

            // Now, do it all in one call.
            state = new RngState128(s0, s1);
            buf.Clear();
            gen.FillBuffer(state, buf);
            Assert.True(expectedResults.SequenceEqual(actualResults));
        }
    }
}
