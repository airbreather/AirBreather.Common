using System;
using System.Diagnostics.CodeAnalysis;

namespace AirBreather.Common.Random
{
    public struct XorShift128PlusState : IEquatable<XorShift128PlusState>, IRandomGeneratorState
    {
        internal ulong s0;
        internal ulong s1;

        public XorShift128PlusState(ulong s0, ulong s1)
        {
            if (s0 == 0 && s1 == 0)
            {
                throw new ArgumentException("At least one seed value must be non-zero.");
            }

            this.s0 = s0;
            this.s1 = s1;
        }

        public XorShift128PlusState(XorShift128PlusState copyFrom)
            : this(copyFrom.s0, copyFrom.s1)
        {
        }

        public bool IsValid => this.s0 != 0 || this.s1 != 0;

        public static bool Equals(XorShift128PlusState first, XorShift128PlusState second) => first.s0 == second.s0 && first.s1 == second.s1;
        public static int GetHashCode(XorShift128PlusState state) => (state.s0 ^ state.s1).GetHashCode();

        public static bool operator ==(XorShift128PlusState first, XorShift128PlusState second) => Equals(first, second);
        public static bool operator !=(XorShift128PlusState first, XorShift128PlusState second) => !Equals(first, second);
        public override bool Equals(object obj) => obj is XorShift128PlusState && Equals(this, (XorShift128PlusState)obj);
        public bool Equals(XorShift128PlusState other) => Equals(this, other);
        public override int GetHashCode() => GetHashCode(this);
    }

    public sealed class XorShift128PlusGenerator : IRandomGenerator<XorShift128PlusState>
    {
        /// <summary>
        /// The size of each "chunk" of bytes that can be generated at a time.
        /// </summary>
        public static readonly int ChunkSize = sizeof(ulong);

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        int IRandomGenerator<XorShift128PlusState>.ChunkSize => ChunkSize;

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        RandomnessKind IRandomGenerator<XorShift128PlusState>.RandomnessKind => RandomnessKind.PseudoRandom;

        /// <inheritdoc />
        public XorShift128PlusState FillBuffer(XorShift128PlusState state, byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Must be non-negative.");
            }

            if (buffer.Length <= index)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Must be less than the length of the buffer.");
            }

            if (buffer.Length - index < count)
            {
                throw new ArgumentException("Not enough room", nameof(buffer));
            }

            if (count % ChunkSize != 0)
            {
                throw new ArgumentException("Must be a multiple of ChunkSize.", nameof(count));
            }

            if (!state.IsValid)
            {
                throw new ArgumentException("State is not valid; use the parameterized constructor to initialize a new instance with the given seed values.", nameof(state));
            }

            return FillBufferCore(state, buffer, count);
        }

        private static unsafe XorShift128PlusState FillBufferCore(XorShift128PlusState state, byte[] buffer, int count)
        {
            fixed (byte* fbuf = buffer)
            {
                // count has already been validated to be a multiple of ChunkSize,
                // so we can do this fanciness without fear.
                ulong* pbuf = (ulong*)fbuf;
                ulong* pend = pbuf + (count / ChunkSize);
                while (pbuf < pend)
                {
                    ulong s1 = state.s0;
                    ulong s0 = state.s1;
                    state.s0 = s0;
                    s1 ^= s1 << 23;
                    *(pbuf++) = unchecked((state.s1 = (s1 ^ s0 ^ (s1 >> 17) ^ (s0 >> 26))) + s0);
                }
            }

            return state;
        }
    }
}
