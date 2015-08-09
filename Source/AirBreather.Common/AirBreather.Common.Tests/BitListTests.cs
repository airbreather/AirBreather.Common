using System;
using System.Collections;
using System.Linq;

using AirBreather.Common.Collections;

using Xunit;

namespace AirBreather.Common.Tests
{
    public sealed class BitListTests
    {
        [Fact]
        public void ThrowingForBadArgs()
        {
            BitList bl = new BitList { true, true, false, false };

            Assert.Throws<ArgumentOutOfRangeException>("index", () => bl[-1]);
            Assert.Throws<ArgumentOutOfRangeException>("index", () => bl[4]);
            Assert.Throws<ArgumentOutOfRangeException>("index", () => bl[-1] = false);
            Assert.Throws<ArgumentOutOfRangeException>("index", () => bl[4] = false);
            Assert.Throws<ArgumentOutOfRangeException>("index", () => bl.Insert(-1, false));
            Assert.Throws<ArgumentOutOfRangeException>("index", () => bl.Insert(5, true));
            Assert.Throws<ArgumentOutOfRangeException>("index", () => bl.RemoveAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>("index", () => bl.RemoveAt(4));
            Assert.Throws<ArgumentNullException>("array", () => bl.CopyTo(null, 5));
            Assert.Throws<ArgumentNullException>("bitArray", () => new BitList(null));

            bool[] things = new bool[5];
            Assert.Throws<ArgumentOutOfRangeException>("arrayIndex", () => bl.CopyTo(things, -1));
            Assert.Throws<ArgumentException>("array", () => bl.CopyTo(things, 2));
            Assert.Throws<ArgumentOutOfRangeException>("arrayIndex", () => bl.CopyTo(things, 5));

            things = new bool[3];
            Assert.Throws<ArgumentException>("array", () => bl.CopyTo(things, 0));
            Assert.Throws<ArgumentOutOfRangeException>("arrayIndex", () => bl.CopyTo(things, 3));
        }

        [Fact]
        public void AddAndRemoveShouldBeConsistent()
        {
            BitList bl = new BitList { false, false };

            Assert.Equal(false, bl[0]);
            Assert.Equal(2, bl.Count);

            Assert.False(bl.Remove(true));
            Assert.Equal(2, bl.Count);

            Assert.True(bl.Remove(false));

            Assert.Equal(1, bl.Count);
        }

        [Fact]
        public void InsertAndRemoveAtShouldBeConsistent()
        {
            BitList bl = new BitList { true, false, true };

            bl.Insert(1, false);

            Assert.Equal(4, bl.Count);

            bool[] expected = { true, false, false, true };
            Assert.Equal(expected, bl);

            bl.RemoveAt(2);

            Assert.Equal(3, bl.Count);
            expected = new[] { true, false, true };
            Assert.Equal(expected, bl);
        }

        [Fact]
        public void ShouldNotBeReadOnly()
        {
            BitList bl = new BitList();

            Assert.False(bl.IsReadOnly);
        }

        [Fact]
        public void Contains()
        {
            BitList bl = new BitList();

            Assert.False(bl.Contains(true));
            Assert.False(bl.Contains(false));

            bl.Add(true);

            Assert.Equal(1, bl.Count);
            Assert.True(bl.Contains(true));
            Assert.False(bl.Contains(false));

            bl.Clear();
            bl.TrimExcess();

            Assert.Equal(0, bl.Count);
            Assert.False(bl.Contains(true));
            Assert.False(bl.Contains(false));

            bl.Insert(0, false);
            Assert.Equal(1, bl.Count);
            Assert.False(bl.Contains(true));
            Assert.True(bl.Contains(false));

            bl[0] = true;
            Assert.Equal(1, bl.Count);
            Assert.True(bl.Contains(true));
            Assert.False(bl.Contains(false));

            bl[0] = false;
            Assert.Equal(1, bl.Count);
            Assert.False(bl.Contains(true));
            Assert.True(bl.Contains(false));
        }

        [Fact]
        public void CopyToShouldWork()
        {
            const int ByteCount = 1927;
            const int BoolCount = ByteCount * 8;
            const int ArrayOffset = 3871;

            byte[] someStuff = new byte[ByteCount];
            new System.Random().NextBytes(someStuff);

            BitList bl = new BitList();
            foreach (byte val in someStuff)
            {
                for (int j = 0; j < 8; j++)
                {
                    bl.Add((val & (1 << j)) > 0);
                }
            }

            bool[] vals = new bool[BoolCount + ArrayOffset];
            bl.CopyTo(vals, ArrayOffset);

            Assert.Equal(bl, vals.Skip(ArrayOffset));
        }

        [Fact]
        public void ToBitArrayShouldWork()
        {
            const int ByteCount = 1927;

            byte[] someStuff = new byte[ByteCount];
            new System.Random().NextBytes(someStuff);

            BitList bl = new BitList();
            foreach (byte val in someStuff)
            {
                for (int j = 0; j < 8; j++)
                {
                    bl.Add((val & (1 << j)) > 0);
                }
            }

            BitArray ba = bl.ToBitArray();

            Assert.Equal(bl.Count, ba.Length);
            for (int i = 0; i < bl.Count; i++)
            {
                Assert.Equal(bl[i], ba[i]);
            }
        }

        [Theory]
        [InlineData(32 * 60)]
        [InlineData(32 * 12 + 19)]
        public void CopyFromBitArrayShouldWork(int byteCount)
        {
            byte[] someStuff = new byte[byteCount];
            new System.Random().NextBytes(someStuff);
            BitArray ba = new BitArray(someStuff);
            BitList bl = new BitList(ba);

            for (int i = 0; i < ba.Length; i++)
            {
                Assert.Equal(ba[i], bl[i]);
            }
        }
    }
}
