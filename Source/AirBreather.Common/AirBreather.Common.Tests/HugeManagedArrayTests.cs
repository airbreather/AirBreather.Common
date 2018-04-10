using System;
using System.Runtime.InteropServices;

using AirBreather.Collections;
using AirBreather.Random;

using Xunit;

namespace AirBreather.Tests
{
    public sealed class HugeManagedArrayTests
    {
        [Fact]
        public void EqualityTests()
        {
            ulong[] data = new ulong[50124];
            CryptographicRandomGenerator.FillBuffer(MemoryMarshal.AsBytes(data.AsSpan()));

            HugeManagedArray<ulong> arrMine = new HugeManagedArray<ulong>(data);

            // IEnumerable<T>
            Assert.Equal(data, arrMine);

            // simple indexing
            for (int i = 0; i < data.Length; i++)
            {
                Assert.Equal(data[i], arrMine[i]);
            }

            // pattern foreach
            int j = 0;
            foreach (ulong val in data)
            {
                Assert.Equal(val, arrMine[j++]);
            }

            Assert.Equal(j, arrMine.Length);
        }

        private struct EnumContainer
        {
            private DateTimeKind? field;
        }
    }
}
