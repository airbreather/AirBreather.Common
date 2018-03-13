using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

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

        // TODO: this (and what it came from) is a bit overcomplicated of a way of saying 1024
        // "yes / no" answers in a specific sequence... maybe just write out that sequence of
        // 1024 yes / no answers as a bool[] or BitArray or something.
        private static readonly ulong[] JumpMasks = { 0x84242F96ECA9C41D, 0xA3C65B8776F96855, 0x5B34A39F070B5837, 0x4489AFFCE4F31A1E, 0x2FFEEB0A48316F40, 0xDC2D9891FE68C022, 0x3659132BB12FEA70, 0xAAC17D8EFA43CAB8, 0xC4CB815590989B13, 0x5EE975283D71C93B, 0x691548C86C1BD540, 0x7910C41D10A1E6A5, 0x0B5FC64563B3E2A8, 0x047F7684E9FC949D, 0xB99181F2D8F685CA, 0x284600E3F30E38C3 };

        public static unsafe RngState1024 Jump(RngState1024 state)
        {
            if (!state.IsValid)
            {
                throw new ArgumentException("State is not valid; use the parameterized constructor to initialize a new instance with the given seed values.", nameof(state));
            }

            ulong* pTmp = stackalloc ulong[16];
            ulong* pState = (ulong*)&state;

            for (int i = 0; i < JumpMasks.Length; ++i)
            {
                ulong msk = JumpMasks[i];
                for (int b = 0; b < 64; ++b)
                {
                    if ((msk & (1UL << b)) != 0)
                    {
                        for (int j = 0; j < 16; ++j)
                        {
                            pTmp[j] ^= pState[(j + state.p) & 15];
                        }
                    }

                    ulong s0 = pState[state.p];
                    ulong s1 = pState[state.p = ((state.p + 1) & 15)];

                    s1 ^= s1 << 31;
                    pState[state.p] = (s0 ^ (s0 >> 30)) ^ (s1 ^ (s1 >> 11));
                }
            }

            for (int j = 0; j < 16; ++j)
            {
                pState[(j + state.p) & 15] = pTmp[j];
            }

            return state;
        }

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

            ulong* pState = (ulong*)&state;
            fixed (byte* fbuf = &MemoryMarshal.GetReference(buffer))
            {
                // count has already been validated to be a multiple of ChunkSize,
                // and we assume index is OK too, so we can do this fanciness without fear.
                for (ulong* pbuf = (ulong*)fbuf, pend = pbuf + (buffer.Length / ChunkSize); pbuf < pend; ++pbuf)
                {
                    ulong s0 = pState[state.p];
                    ulong s1 = pState[state.p = ((state.p + 1) & 15)];

                    s1 ^= s1 << 31;
                    *pbuf = unchecked(1181783497276652981 * (pState[state.p] = (s0 ^ (s0 >> 30)) ^ (s1 ^ (s1 >> 11))));
                }
            }

            return state;
        }
    }
}
