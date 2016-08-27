using System;
using System.Diagnostics.CodeAnalysis;

using AirBreather.Common.Utilities;

namespace AirBreather.Common.Random
{
    public sealed class XorShift128PlusGenerator : IRandomGenerator<RngState128>
    {
        /// <summary>
        /// The size of each "chunk" of bytes that can be generated at a time.
        /// </summary>
        public static readonly int ChunkSize = sizeof(ulong);

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        int IRandomGenerator<RngState128>.ChunkSize => ChunkSize;

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        RandomnessKind IRandomGenerator<RngState128>.RandomnessKind => RandomnessKind.PseudoRandom;

        /// <inheritdoc />
        public RngState128 FillBuffer(RngState128 state, byte[] buffer, int index, int count)
        {
            buffer.ValidateNotNull(nameof(buffer));
            index.ValidateInRange(nameof(index), 0, buffer.Length);

            if (buffer.Length - index < count)
            {
                throw new ArgumentException("Not enough room", nameof(buffer));
            }

            if (index % ChunkSize != 0)
            {
                throw new ArgumentException("Must be a multiple of ChunkSize.", nameof(index));
            }

            if (count % ChunkSize != 0)
            {
                throw new ArgumentException("Must be a multiple of ChunkSize.", nameof(count));
            }

            if (!state.IsValid)
            {
                throw new ArgumentException("State is not valid; use the parameterized constructor to initialize a new instance with the given seed values.", nameof(state));
            }

            return FillBufferCore(state, buffer, index, count);
        }

        private static unsafe RngState128 FillBufferCore(RngState128 state, byte[] buffer, int index, int count)
        {
            fixed (byte* fbuf = buffer)
            {
                // count has already been validated to be a multiple of ChunkSize,
                // and so has index, so we can do this fanciness without fear.
                ulong* pbuf = (ulong*)(fbuf + index);
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
