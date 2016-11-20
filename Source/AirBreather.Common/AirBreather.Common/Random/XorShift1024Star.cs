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
        public RngState1024 FillBuffer(RngState1024 state, byte[] buffer, int index, int count)
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

            FillBufferCore(ref state);
            return state;

            unsafe void FillBufferCore(ref RngState1024 state2)
            {
                fixed (byte* fbuf = buffer)
                fixed (RngState1024* fState = &state2)
                {
                    // count has already been validated to be a multiple of ChunkSize,
                    // and so has index, so we can do this fanciness without fear.
                    ulong* pbuf = (ulong*)(fbuf + index);
                    ulong* pend = pbuf + (count / ChunkSize);

                    ulong* pState = (ulong*)fState;

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
            }
        }
    }
}
