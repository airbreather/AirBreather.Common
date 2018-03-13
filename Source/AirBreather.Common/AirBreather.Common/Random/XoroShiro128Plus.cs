// Ported from xoroshiro128plus.c, which contains this header:
/*  Written in 2016 by David Blackman and Sebastiano Vigna (vigna@acm.org)

To the extent possible under law, the author has dedicated all copyright
and related and neighboring rights to this software to the public domain
worldwide. This software is distributed without any warranty.

See <http://creativecommons.org/publicdomain/zero/1.0/>. */
/* This is the successor to xorshift128+. It is the fastest full-period
   generator passing BigCrush without systematic failures, but due to the
   relatively short period it is acceptable only for applications with a
   mild amount of parallelism; otherwise, use a xorshift1024* generator.

   Beside passing BigCrush, this generator passes the PractRand test suite
   up to (and included) 16TB, with the exception of binary rank tests,
   which fail due to the lowest bit being an LFSR; all other bits pass all
   tests.
   
   Note that the generator uses a simulated rotate operation, which most C
   compilers will turn into a single instruction. In Java, you can use
   Long.rotateLeft(). In languages that do not make low-level rotation
   instructions accessible xorshift128+ could be faster.

   The state must be seeded so that it is not everywhere zero. If you have
   a 64-bit seed, we suggest to seed a splitmix64 generator and use its
   output to fill s. */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace AirBreather.Random
{
    // TODO: fully replace XorShift128Plus with this, once it's been proven out more rigorously.
    public sealed class XoroShiro128PlusGenerator : IRandomGenerator<RngState128>
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

            // TODO: this (and what it came from) is a bit overcomplicated of a way of saying 128
            // "yes / no" answers in a specific sequence... maybe just write out that sequence of
            // 128 yes / no answers as a bool[] or BitArray or something.
            ulong msk = 0xBEAC0467EBA5FACB;
            for (int i = 0; i < 128; ++i)
            {
                if (i == 64)
                {
                    msk = 0xD86B048B86AA9922;
                }

                if ((msk & (1UL << (i & 0x3F))) != 0)
                {
                    result.s0 ^= state.s0;
                    result.s1 ^= state.s1;
                }

                ulong t = state.s0 ^ state.s1;
                state.s0 = ((state.s0 << 55) | (state.s0 >> 9)) ^ t ^ (t << 14);
                state.s1 = (t << 36) | (t >> 28);
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
                    *pbuf = unchecked(state.s0 + state.s1);

                    ulong t = state.s0 ^ state.s1;
                    state.s0 = ((state.s0 << 55) | (state.s0 >> 9)) ^ t ^ (t << 14);
                    state.s1 = (t << 36) | (t >> 28);
                }
            }

            return state;
        }
    }
}
