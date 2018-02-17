using System;
using System.Collections.Immutable;
using AirBreather.Collections;
using AirBreather.Random;
using Xunit;

namespace AirBreather.Tests
{
    public sealed class HugeManagedArrayTests
    {
        [Fact]
        public void MustBeBlittable()
        {
            Assert.Throws<ArgumentException>(() => new HugeManagedArray<ImmutableArray<byte>>(1));
            new HugeManagedArray<EnumContainer>(1);
        }

        [Fact]
        public void EqualityTests()
        {
            ulong[] data = new ulong[50124];
            Span<byte> asBytes = data.AsSpan().AsBytes();

            byte[] stupidByteArray = new byte[asBytes.Length];
            CryptographicRandomGenerator.FillBuffer(stupidByteArray);
            stupidByteArray.AsReadOnlySpan().CopyTo(asBytes);

            HugeManagedArray<ulong> arrMine = new HugeManagedArray<ulong>(data);

            // IEnumerable<T>
            Assert.Equal(data, arrMine);

            // simple indexing
            for (int i = 0; i < data.Length; i++)
            {
                Assert.Equal(data[i], arrMine[i]);
            }

            // slicing
            Assert.True(arrMine.Slice(0, data.Length).SequenceEqual(data));
        }

        private struct EnumContainer
        {
            private DateTimeKind? field;
        }
    }
}
