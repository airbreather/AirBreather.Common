using System;
using System.Diagnostics.CodeAnalysis;

namespace AirBreather.Random
{
    public sealed class XorShift1024StarGenerator : IRandomGenerator<RngState1024>
    {
        /// <summary>
        /// The size of each "chunk" of bytes that can be generated at a time.
        /// </summary>
        public static readonly int ChunkSize = sizeof(ulong);

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        int IRandomGenerator<RngState1024>.ChunkSize => ChunkSize;

        /// <inheritdoc />
        public unsafe RngState1024 FillBuffer(RngState1024 state, Span<byte> buffer)
        {
            if (buffer.Length % ChunkSize != 0)
            {
                throw new ArgumentException("Length must be a multiple of ChunkSize.", nameof(buffer));
            }

            if (!state.IsValid)
            {
                throw new ArgumentException("State is not valid; use the parameterized constructor to initialize a new instance with the given seed values.", nameof(state));
            }

            fixed (byte* fbuf = &buffer.DangerousGetPinnableReference())
            {
                // count has already been validated to be a multiple of ChunkSize,
                // and we assume index is OK too, so we can do this fanciness without fear.
                ulong* pbuf = (ulong*)fbuf;
                ulong* pend = pbuf + (buffer.Length / ChunkSize);

                ulong* pState = (ulong*)&state;

                while (pbuf < pend)
                {
                    ulong s0 = *(pState + (state.p++));
                    state.p = state.p & 15;

                    ulong* pCurr = pState + state.p;
                    ulong s1 = *(pCurr);

                    s1 ^= s1 << 31;
                    s1 ^= s1 >> 11;
                    s0 ^= s0 >> 30;
                    *pCurr = s0 ^ s1;
                    *(pbuf++) = unchecked(1181783497276652981 * *(pCurr));
                }
            }

            return state;
        }
    }
}
