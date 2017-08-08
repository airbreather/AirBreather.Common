using System;
using System.Collections.Generic;

using AirBreather.Text;

using Xunit;

namespace AirBreather.Tests
{
    public sealed class UTF8StringTests
    {
        // no InlineData, see: https://codeblog.jonskeet.uk/2014/11/07/when-is-a-string-not-a-string
        // it's better this way anyway...
        [Theory]
        [MemberData(nameof(Strings))]
        public void MatchAllocatingBuiltin(string orig)
        {
            byte[] data = EncodingEx.UTF8NoBOM.GetBytes(orig);
            UTF8String sut = orig;

            Assert.True(SpanExtensions.SequenceEqual(data, sut.EncodedData));
            Assert.Equal(orig, sut);
        }

        [Theory]
        [MemberData(nameof(HashCodes))]
        public void HashCodesUseMurmur3(string input, int seed, int expected)
        {
            UTF8String sut = input;
            Assert.Equal(expected, sut.GetHashCode(seed));
            if (seed == 0)
            {
                Assert.Equal(expected, sut.GetHashCode());
            }
        }

        public static IEnumerable<object[]> Strings => new[]
        {
            new object[] { String.Empty },
            new object[] { "fhqwhgadshgnsdhjsdbkhsdabkfabkveybvf" },
            new object[] { "my 0.02\u00a2: Verizon math is an entertaining series of blog posts" },
            new object[] { "would you like to test a \u2603?  okay, bye" },
            new object[] { "don\u00e2\u20ac\u2122t worry, I tested my program with funny symbols" },
            new object[] { "cute \U0001F428 on the outside, drop bear on the inside" }
        };

        // http://stackoverflow.com/a/31929528/1083771
        public static IEnumerable<object[]> HashCodes => new[]
        {
            TestString("", 0, 0), //empty string with zero seed should give zero
            TestString("", 1, 0x514E28B7),
            TestString("", 0xffffffff, 0x81F16F39), //make sure seed value is handled unsigned
            TestString("\0\0\0\0", 0, 0x2362F9DE), //make sure we handle embedded nulls

            TestString("aaaa", 0x9747b28c, 0x5A97808A), //one full chunk
            TestString("aaa", 0x9747b28c, 0x283E0130), //three characters
            TestString("aa", 0x9747b28c, 0x5D211726), //two characters
            TestString("a", 0x9747b28c, 0x7FA09EA6), //one character

            //Endian order within the chunks
            TestString("abcd", 0x9747b28c, 0xF0478627), //one full chunk
            TestString("abc", 0x9747b28c, 0xC84A62DD),
            TestString("ab", 0x9747b28c, 0x74875592),

            TestString("Hello, world!", 0x9747b28c, 0x24884CBA),

            //Make sure you handle UTF-8 high characters. A bcrypt implementation messed this up
            TestString(new string('\u03c0', 8), 0x9747b28c, 0xD58063C1), //U+03C0: Greek Small Letter Pi

            //String of 256 characters.
            //Make sure you don't store string lengths in a char, and overflow at 255 bytes (as OpenBSD's canonical BCrypt implementation did)
            TestString(new string('a', 256), 0x9747b28c, 0x37405BDC),

            //"I'll post just two of the 11 SHA-2 test vectors that i converted to Murmur3."
            TestString("abc", 0, 0xB3DD93FA),
            TestString("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq", 0, 0xEE925B90),

            //And finally, the big one:
            TestString("The quick brown fox jumps over the lazy dog", 0x9747b28c, 0x2FA826CD)
        };

        private static object[] TestString(string input, uint seed, uint result) => new object[] { input, unchecked((int)seed), unchecked((int)result) };
    }
}
