using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
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
            byte[] arrBase = CryptographicRandomGenerator.FillBuffer(new byte[9999]);
            HugeManagedArray<byte> arrMine = new HugeManagedArray<byte>(9999);
            arrBase.AsReadOnlySpan().CopyTo(arrMine.Slice(0, 9999));

            // IEnumerable<T>
            Assert.Equal(arrBase, arrMine);

            // simple indexing
            for (int i = 0; i < arrBase.Length; i++)
            {
                Assert.Equal(arrBase[i], arrMine[i]);
            }

            // slicing
            Assert.True(arrMine.Slice(0, 9999).SequenceEqual(arrBase));
        }

        private struct EnumContainer
        {
            private DateTimeKind? field;
        }
    }
}
