using System;

using AirBreather.Common.Utilities;

namespace AirBreather.Common.Random
{
    public sealed class RandomPicker<TState> : IPicker where TState : struct, IRandomGeneratorState
    {
        // Seems about right, from testing various sizes.
        private const int DesiredBufferSizeInBytes = 16384;

        private readonly IRandomGenerator<TState> rng;

        private readonly byte[] buffer;

        private readonly object lockObject = new object();

        private TState rngState;

        private int nextOffset;

        public RandomPicker(IRandomGenerator<TState> rng, TState initialState)
        {
            rng.ValidateNotNull(nameof(rng));

            if (!initialState.IsValid)
            {
                throw new ArgumentException("Initial state must be valid.", nameof(initialState));
            }

            this.rng = rng;
            this.rngState = initialState;

            int chunkSize = rng.ChunkSize;
            int extra = DesiredBufferSizeInBytes % chunkSize;
            this.buffer = new byte[DesiredBufferSizeInBytes + (extra == 0 ? 0 : chunkSize - extra)];
        }

        public int PickFromRange(int minValueInclusive, int rangeSize)
        {
            if (rangeSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(rangeSize), rangeSize, "Must be positive");
            }

            if (Int32.MaxValue - rangeSize < minValueInclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(minValueInclusive), minValueInclusive, "Must be small enough to avoid overflow");
            }

            int bufferLength = this.buffer.Length;

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
            // can't add 1 to Int32.MaxValue.
            int rerollThreshold = ((Int32.MaxValue % rangeSize) + 1) % rangeSize;
            int sample;
            lock (this.lockObject)
            {
                int offset = this.nextOffset;
                do
                {
                    if (offset == 0)
                    {
                        this.rngState = this.rng.FillBuffer(this.rngState, this.buffer, 0, bufferLength);
                    }

                    sample = BitConverter.ToInt32(this.buffer, offset);

                    // range of sample is [Int32.MinValue, Int32.MaxValue].
                    // we need [0, Int32.MaxValue - rerollThreshold].
                    // for the left half, we can just mask away the first bit.
                    // it's silly to discard a perfectly good random bit,
                    // but it's even sillier to bend over backwards to save it.
                    sample &= 0x7FFFFFFF;
                    offset = (offset + 4) % bufferLength;
                }
                while (Int32.MaxValue - rerollThreshold < sample);

                this.nextOffset = offset;
            }

            sample %= rangeSize;
            return minValueInclusive + sample;
        }
    }
}