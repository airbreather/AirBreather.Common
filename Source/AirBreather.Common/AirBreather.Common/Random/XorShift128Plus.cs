using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace AirBreather.Random
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

        public static RngState128 Jump(RngState128 state)
        {
            if (!state.IsValid)
            {
                throw new ArgumentException("State is not valid; use the parameterized constructor to initialize a new instance with the given seed values.", nameof(state));
            }

            RngState128 result = default;

            ulong msk = 0x8A5CD789635D2DFF;
            for (int i = 0; i < 128; ++i)
            {
                if (i == 64)
                {
                    msk = 0x121FD2155C472F96;
                }

                if ((msk & (1UL << (i & 0x3F))) != 0)
                {
                    result.s0 ^= state.s0;
                    result.s1 ^= state.s1;
                }

                ulong s1 = state.s0;
                ulong s0 = state.s1;
                state.s0 = s0;
                s1 ^= s1 << 23;
                state.s1 = (s1 ^ s0 ^ (s1 >> 17) ^ (s0 >> 26));
            }

            return result;
        }

        /// <inheritdoc />
        public unsafe RngState128 FillBuffer(RngState128 state, Span<byte> buffer)
        {
            if (buffer.Length % ChunkSize != 0)
            {
                throw new ArgumentException("Length must be a multiple of ChunkSize.", nameof(buffer));
            }

            if (!state.IsValid)
            {
                throw new ArgumentException("State is not valid; use the parameterized constructor to initialize a new instance with the given seed values.", nameof(state));
            }

            fixed (byte* fbuf = &MemoryMarshal.GetReference(buffer))
            {
                // count has already been validated to be a multiple of ChunkSize,
                // and we assume index is OK too, so we can do this fanciness without fear.
                for (ulong* pbuf = (ulong*)fbuf, pend = pbuf + (buffer.Length / ChunkSize); pbuf < pend; ++pbuf)
                {
                    ulong s1 = state.s0;
                    ulong s0 = state.s1;
                    state.s0 = s0;
                    s1 ^= s1 << 23;
                    *pbuf = unchecked((state.s1 = (s1 ^ s0 ^ (s1 >> 17) ^ (s0 >> 26))) + s0);
                }
            }

            return state;
        }
    }
}
