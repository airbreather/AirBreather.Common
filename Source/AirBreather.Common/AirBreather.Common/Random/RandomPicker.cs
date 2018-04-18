using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace AirBreather.Random
{
    public sealed class RandomPicker<TState> : IPicker where TState : IRandomGeneratorState
    {
        // Seems about right, from testing various sizes.
        private const int BufferLengthBytes = 16384;

        private const int BufferLength = BufferLengthBytes / sizeof(int);

        private readonly IRandomGenerator<TState> rng;

        private readonly int[] buffer;

        private TState rngState;

        private int nextOffset;

        public RandomPicker(IRandomGenerator<TState> rng, TState initialState)
        {
            rng.ValidateNotNull(nameof(rng));

            if (initialState == null)
            {
                throw new ArgumentNullException(nameof(initialState));
            }

            if (!initialState.IsValid)
            {
                throw new ArgumentException("Initial state must be valid.", nameof(initialState));
            }

            if (BufferLengthBytes % rng.ChunkSize != 0)
            {
                throw new NotSupportedException("Chunk size must evenly divide " + BufferLengthBytes.ToString(CultureInfo.InvariantCulture));
            }

            this.rng = rng;
            this.rngState = initialState;
            this.buffer = new int[BufferLength];
        }

        public int PickFromRange(int minValueInclusive, int rangeSize)
        {
            rangeSize.ValidateNotLessThan(nameof(rangeSize), 1);
            (Int32.MaxValue - rangeSize).ValidateNotLessThan(nameof(minValueInclusive), minValueInclusive);

            // Conceptually, this creates several "buckets", each with values
            // [0, rangeSize) in it, plus a bucket with values [0, n), where
            // n is in [1, rangeSize].  This is because (2^31 - 1) isn't evenly
            // divisible by all nonnegative integers smaller than it.  So we
            // reroll a number if it lands in a bucket where n < rangeSize.
            // The tricky part is figuring out the reroll threshold.  I just
            // used 3-bit integers for simplicity, then extended the math up.
            // rangeSize = 0-5, total range = 0-7.  Buckets are [0-4], [5-7].
            // We want to exclude the upper bucket, which contains 3 values.
            // 3 came from 8 % 5, so what we want is (MaxValue + 1) % rangeSize.
            // We have to do a little bit of a dance, though, since we obviously
            // can't add 1 to Int32.MaxValue in signed 32-bit arithmetic.
            int rerollThreshold = unchecked((int)((Int32.MaxValue + 1U) % (uint)rangeSize));
            int sample;
            lock (this.buffer)
            {
                do
                {
                    if (this.nextOffset == 0)
                    {
                        this.rngState = this.rng.FillBuffer(this.rngState, MemoryMarshal.AsBytes(this.buffer.AsSpan()));
                    }

                    sample = this.buffer[this.nextOffset++];

                    // range of sample is [Int32.MinValue, Int32.MaxValue].
                    // we need [0, Int32.MaxValue - rerollThreshold].
                    // for the left half, we can just mask away the first bit.
                    // it's silly to discard a perfectly good random bit,
                    // but it's even sillier to bend over backwards to save it.
                    sample &= 0x7FFFFFFF;
                    this.nextOffset %= BufferLength;
                }
                while (Int32.MaxValue - rerollThreshold < sample);
            }

            sample %= rangeSize;
            return minValueInclusive + sample;
        }
    }
}
