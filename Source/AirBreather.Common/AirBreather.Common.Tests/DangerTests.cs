using System.Collections.Immutable;
using System.IO;

using AirBreather.Danger;
using AirBreather.Random;

using Xunit;

namespace AirBreather.Tests
{
    public class DangerTests
    {
        [Fact]
        public void ExactlyWhatYouAreNotSupposedToDo()
        {
            // i r ImmutableArray<T>.Builder nao, look at me
            ulong[] origBuf = { 1, 2, 3, 4 };
            ImmutableArray<ulong> fakeImmutableBuf = origBuf.AsImmutableArrayDangerous();

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
            var bld = ImmutableArray.CreateBuilder<byte>(80000);
            for (int i = 0; i < 80000; ++i)
            {
                bld.Add(CryptographicRandomGenerator.NextByte());
            }

            var data = bld.ToImmutable();
            using (var dst = new MemoryStream())
            {
                using (var src = data.ToReadableStream())
                {
                    src.CopyTo(dst);
                }

                Assert.Equal(data, dst.ToArray());
            }
        }
    }
}
