using System;

using AirBreather.Random;

using Xunit;

namespace AirBreather.Tests
{
    public sealed class MiscTests
    {
        [Fact]
        public void TestEqualsData()
        {
            var buf1 = CryptographicRandomGenerator.GetBuffer(80000);
            var buf2 = new byte[80000];
            new ReadOnlySpan<byte>(buf1).CopyTo(buf2);

            Assert.True(buf1.EqualsData(buf2));

            unchecked
            {
                ++buf1[65000];
            }

            Assert.False(buf1.EqualsData(buf2));
        }
    }
}
