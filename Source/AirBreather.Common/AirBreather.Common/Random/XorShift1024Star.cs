using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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
        public RngState1024 FillBuffer(RngState1024 state, Span<byte> buffer)
        {
            if (buffer.Length % ChunkSize != 0)
            {
                throw new ArgumentException("Length must be a multiple of ChunkSize.", nameof(buffer));
            }

            if (!state.IsValid)
            {
                throw new ArgumentException("State is not valid; use the parameterized constructor to initialize a new instance with the given seed values.", nameof(state));
            }

            ref ulong rState = ref Unsafe.As<RngState1024, ulong>(ref state);
            Span<ulong> chunkBuffer = buffer.NonPortableCast<byte, ulong>();
            for (int i = 0; i < chunkBuffer.Length; ++i)
            {
                ulong s0 = Unsafe.Add(ref rState, state.p++);
                state.p = state.p & 15;

                ref ulong s1 = ref Unsafe.Add(ref rState, state.p);

                s1 ^= s1 << 31;
                s1 ^= s1 >> 11;
                s1 ^= s0 >> 30;
                chunkBuffer[i] = unchecked(1181783497276652981 * s1);
            }

            return state;
        }
    }
}
