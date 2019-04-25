using System;
using System.IO;

using AirBreather.Danger;
using AirBreather.Random;

using Xunit;

namespace AirBreather.Tests
{
    public sealed class DangerTests
    {
        [Fact]
        public void ExactlyWhatYouAreNotSupposedToDo()
        {
            // i r ImmutableArray<T>.Builder nao, look at me
            ulong[] origBuf = { 1, 2, 3, 4 };
            var fakeImmutableBuf = origBuf.AsImmutableArrayDangerous();

            // we should get the same values.
            Assert.Equal(origBuf, fakeImmutableBuf);

            // it should also be exactly the same instance... this MUST be copy-free, or else it's
            // not worth it.
            Assert.Same(origBuf, fakeImmutableBuf.AsRegularArrayDangerous());

            // this is exactly what you're not supposed to do.
            ++origBuf[2];
            Assert.Equal(origBuf, fakeImmutableBuf);
        }

        [Fact]
        public void TestToReadableStream()
        {
            var data = CryptographicRandomGenerator.GetBuffer(3500).AsImmutableArrayDangerous();

            using (var dst = new MemoryStream())
            {
                using (var src = data.ToReadableStream())
                {
                    src.CopyTo(dst);
                }

                Assert.True(data.AsReadOnlySpan().SequenceEqual(dst.ToArray()));
            }
        }
    }
}
