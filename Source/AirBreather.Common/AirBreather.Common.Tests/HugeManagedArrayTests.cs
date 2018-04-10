﻿using System;

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
            CryptographicRandomGenerator.FillBuffer(data.AsSpan().AsBytes());

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
