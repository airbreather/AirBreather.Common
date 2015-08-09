using System;

using Xunit;

using AirBreather.Common.Random;

namespace AirBreather.Common.Tests
{
    public sealed class XorShift128PlusTests
    {
        [Theory]
        [InlineData(1234524356ul, 47845723665ul, 10356027574996968ul, 421627830503766283ul, 7267806761253193977ul)]
        [InlineData(262151541652562ul, 468594272265ul, 3923822141990852456ul, 3993942717521754294ul, 13070632098572223408ul)]
        public void Test(ulong s0, ulong s1, ulong expectedResult1, ulong expectedResult2, ulong expectedResult3)
        {
            // this was params ulong[], but I like InlineData too much to make that work...
            ulong[] expectedResults = { expectedResult1, expectedResult2, expectedResult3 };

            var gen = new XorShift128PlusGenerator();
            var state = new XorShift128PlusState(s0, s1);
            byte[] buf = new byte[expectedResults.Length * 8];

            // First, do it in separate calls.
            for (int i = 0; i < expectedResults.Length; i++)
            {
                state = gen.FillBuffer(state, buf, 0, 8);
                Assert.Equal(expectedResults[i], BitConverter.ToUInt64(buf, 0));
            }

            // Now, do it all in one call.
            state = new XorShift128PlusState(s0, s1);
            state = gen.FillBuffer(state, buf, 0, buf.Length);
            for (int i = 0; i < expectedResults.Length; i++)
            {
                Assert.Equal(expectedResults[i], BitConverter.ToUInt64(buf, i * 8));
            }

            // Now, ensure that it throws if we're out of alignment.
            Assert.Throws<ArgumentException>("index", () => state = gen.FillBuffer(state, buf, 3, 8));
        }
    }
}
