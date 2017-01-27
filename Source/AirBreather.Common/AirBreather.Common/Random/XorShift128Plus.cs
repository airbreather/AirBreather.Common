using System;
using System.Diagnostics.CodeAnalysis;

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

        /// <inheritdoc />
        public RngState128 FillBuffer(RngState128 state, Span<byte> buffer)
        {
            if (buffer.Length % ChunkSize != 0)
            {
                throw new ArgumentException("Length must be a multiple of ChunkSize.", nameof(buffer));
            }

            if (!state.IsValid)
            {
                throw new ArgumentException("State is not valid; use the parameterized constructor to initialize a new instance with the given seed values.", nameof(state));
            }

            FillBufferCore();
            return state;

            unsafe void FillBufferCore()
            {
                fixed (byte* fbuf = &buffer.DangerousGetPinnableReference())
                {
                    // count has already been validated to be a multiple of ChunkSize,
                    // and so has index, so we can do this fanciness without fear.
                    ulong* pbuf = (ulong*)fbuf;
                    ulong* pend = pbuf + (buffer.Length / ChunkSize);
                    while (pbuf < pend)
                    {
                        ulong s1 = state.s0;
                        ulong s0 = state.s1;
                        state.s0 = s0;
                        s1 ^= s1 << 23;
                        *(pbuf++) = unchecked((state.s1 = (s1 ^ s0 ^ (s1 >> 17) ^ (s0 >> 26))) + s0);
                    }
                }
            }
        }
    }
}
