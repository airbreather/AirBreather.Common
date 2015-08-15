using System;

using AirBreather.Common.Logging;
using AirBreather.Common.Utilities;

namespace AirBreather.Common.Random
{
    public static class Extensions
    {
        /// <summary>
        /// Fills the specified range of the specified buffer with random or pseudorandom data.
        /// </summary>
        /// <param name="generator">
        /// The <see cref="IRandomGenerator{TState}"/> to use to generate random bytes.
        /// </param>
        /// <param name="state">
        /// The <typeparamref name="TState"/> value that encapsulates the state of the generator.
        /// See remarks for expectations and guarantees.
        /// </param>
        /// <param name="buffer">
        /// A buffer that this method should populate with random bytes.
        /// </param>
        /// <returns>
        /// A <typeparamref name="TState"/> value to use for subsequent calls, if needed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> or <paramref name="generator"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// If <see cref="IRandomGenerator{TState}.RandomnessKind"/> is
        /// <see cref="RandomnessKind.Random"/>, results may not be repeatable (no guarantees are
        /// made about the input or output state).
        /// </para>
        /// <para>
        /// If <see cref="IRandomGenerator{TState}.RandomnessKind"/> is
        /// <see cref="RandomnessKind.PseudoRandom"/>, results for equal <paramref name="state"/>
        /// values will always generate the same sequence of random bytes, and the return value
        /// after generating a given number of bytes will always be the same, no matter how many
        /// calls it took to generate that.  For example, generating 16 bytes, then passing the
        /// resulting state into another call that generates 8 bytes will return the same state as
        /// requesting 24 bytes at once for the original state.
        /// </para>
        /// <para>
        /// Implementations are not <b>required</b> to throw <see cref="ArgumentException"/> when
        /// <paramref name="state"/> is not <see cref="IRandomGeneratorState.IsValid">valid</see>.
        /// </para>
        /// <para>
        /// The entire <paramref name="buffer"/> will be filled.
        /// </para>
        /// </remarks>
        public static TState FillBuffer<TState>(this IRandomGenerator<TState> generator, TState state, byte[] buffer)
            where TState : struct, IRandomGeneratorState
        {
            generator.ValidateNotNull(nameof(generator));
            buffer.ValidateNotNull(nameof(buffer));

            return generator.FillBuffer(state, buffer, 0, buffer.Length);
        }

        public static int NextInt32<TState>(this IRandomGenerator<TState> generator, ref TState state) where TState : struct, IRandomGeneratorState => BitConverter.ToInt32(GetAndFillBuffer(4, generator.ValidateNotNull(nameof(generator)), ref state), 0);
        public static long NextInt64<TState>(this IRandomGenerator<TState> generator, ref TState state) where TState : struct, IRandomGeneratorState => BitConverter.ToInt64(GetAndFillBuffer(8, generator.ValidateNotNull(nameof(generator)), ref state), 0);

        private static byte[] GetAndFillBuffer<TState>(int minimumSize, IRandomGenerator<TState> generator, ref TState state)
            where TState : struct, IRandomGeneratorState
        {
            if (minimumSize < generator.ChunkSize)
            {
                Log.For(typeof(Extensions)).Verbose("Wasting random bytes because the chunk size is greater than requested size.");
                minimumSize = generator.ChunkSize;
            }

            // warning, not particularly nice to the GC to use this too much
            byte[] buffer = new byte[minimumSize];
            state = generator.FillBuffer(state, buffer);
            return buffer;
        }
    }
}
