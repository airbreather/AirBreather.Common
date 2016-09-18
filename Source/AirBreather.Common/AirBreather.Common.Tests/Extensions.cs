using System;

using AirBreather.Random;

namespace AirBreather.Tests
{
    internal static class Extensions
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
            where TState : IRandomGeneratorState
        {
            generator.ValidateNotNull(nameof(generator));
            buffer.ValidateNotNull(nameof(buffer));

            return generator.FillBuffer(state, buffer, 0, buffer.Length);
        }

        // TODO: overload for bool[] or BitArray, whichever makes more sense.

        public static TState FillBuffer<TState>(this IRandomGenerator<TState> generator, TState state, sbyte[] buffer, int index, int count) where TState : IRandomGeneratorState => generator.FillBuffer(state, (byte[])(Array)buffer, index, count);
        public static TState FillBuffer<TState>(this IRandomGenerator<TState> generator, TState state, short[] buffer, int index, int count) where TState : IRandomGeneratorState => FillResizedBuffer(generator, ref state, buffer, index, count, sizeof(short));
        public static TState FillBuffer<TState>(this IRandomGenerator<TState> generator, TState state, ushort[] buffer, int index, int count) where TState : IRandomGeneratorState => FillResizedBuffer(generator, ref state, buffer, index, count, sizeof(ushort));
        public static TState FillBuffer<TState>(this IRandomGenerator<TState> generator, TState state, int[] buffer, int index, int count) where TState : IRandomGeneratorState => FillResizedBuffer(generator, ref state, buffer, index, count, sizeof(int));
        public static TState FillBuffer<TState>(this IRandomGenerator<TState> generator, TState state, uint[] buffer, int index, int count) where TState : IRandomGeneratorState => FillResizedBuffer(generator, ref state, buffer, index, count, sizeof(uint));
        public static TState FillBuffer<TState>(this IRandomGenerator<TState> generator, TState state, long[] buffer, int index, int count) where TState : IRandomGeneratorState => FillResizedBuffer(generator, ref state, buffer, index, count, sizeof(long));
        public static TState FillBuffer<TState>(this IRandomGenerator<TState> generator, TState state, ulong[] buffer, int index, int count) where TState : IRandomGeneratorState => FillResizedBuffer(generator, ref state, buffer, index, count, sizeof(ulong));

        public static TState FillBuffer<TState>(this IRandomGenerator<TState> generator, TState state, sbyte[] buffer) where TState : IRandomGeneratorState => generator.FillBuffer(state, (byte[])(Array)buffer);
        public static TState FillBuffer<TState>(this IRandomGenerator<TState> generator, TState state, short[] buffer) where TState : IRandomGeneratorState => FillResizedBuffer(generator, ref state, buffer, 0, null, sizeof(short));
        public static TState FillBuffer<TState>(this IRandomGenerator<TState> generator, TState state, ushort[] buffer) where TState : IRandomGeneratorState => FillResizedBuffer(generator, ref state, buffer, 0, null, sizeof(ushort));
        public static TState FillBuffer<TState>(this IRandomGenerator<TState> generator, TState state, int[] buffer) where TState : IRandomGeneratorState => FillResizedBuffer(generator, ref state, buffer, 0, null, sizeof(int));
        public static TState FillBuffer<TState>(this IRandomGenerator<TState> generator, TState state, uint[] buffer) where TState : IRandomGeneratorState => FillResizedBuffer(generator, ref state, buffer, 0, null, sizeof(uint));
        public static TState FillBuffer<TState>(this IRandomGenerator<TState> generator, TState state, long[] buffer) where TState : IRandomGeneratorState => FillResizedBuffer(generator, ref state, buffer, 0, null, sizeof(long));
        public static TState FillBuffer<TState>(this IRandomGenerator<TState> generator, TState state, ulong[] buffer) where TState : IRandomGeneratorState => FillResizedBuffer(generator, ref state, buffer, 0, null, sizeof(ulong));

        private static TState FillResizedBuffer<TState>(IRandomGenerator<TState> generator, ref TState state, Array buffer, int index, int? count, int elementSize)
            where TState : IRandomGeneratorState
        {
            generator.ValidateNotNull(nameof(generator));
            buffer.ValidateNotNull(nameof(buffer));
            index.ValidateInRange(nameof(index), 0, buffer.Length);

            int realCount = count ?? buffer.Length;
            realCount.ValidateNotLessThan(nameof(count), 0);

            if (buffer.Length - index < realCount)
            {
                throw new ArgumentException("Not enough room", nameof(buffer));
            }

            int desiredBufferSizeInBytes = realCount * elementSize;
            int chunkSize = generator.ChunkSize;
            int extra = desiredBufferSizeInBytes % chunkSize;
            byte[] byteBuffer = new byte[desiredBufferSizeInBytes + (extra == 0 ? 0 : chunkSize - extra)];

            state = generator.FillBuffer(state, byteBuffer, 0, byteBuffer.Length);
            Buffer.BlockCopy(byteBuffer, 0, buffer, index * elementSize, realCount * elementSize);

            return state;
        }
    }
}
