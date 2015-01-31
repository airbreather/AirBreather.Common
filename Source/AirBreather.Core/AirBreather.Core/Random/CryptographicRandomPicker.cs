using System;
using System.Security.Cryptography;

namespace AirBreather.Core.Random
{
    // Defers to System.Cryptography to get as high-quality random numbers
    // as we possibly can.  I expect to use this mainly for coming up with
    // a seed for a pseudorandom picker.
    public sealed class CryptographicRandomPicker : DisposableObject, IPicker
    {
        // Seems about right, from testing various sizes.
        private const int BufferSizeInBytes = 16384;

        private readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        private readonly byte[] buffer = new byte[BufferSizeInBytes];

        private readonly object lockObject = new object();

        private int nextOffset;

        public int PickFromRange(int minValueInclusive, int rangeSize)
        {
            if (rangeSize < 1)
            {
                throw new ArgumentOutOfRangeException("rangeSize", rangeSize, "Must be positive");
            }

            if (Int32.MaxValue - rangeSize < minValueInclusive)
            {
                throw new ArgumentOutOfRangeException("minValueInclusive", minValueInclusive, "Must be small enough to avoid overflow");
            }

            this.ThrowIfDisposed();

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
                        this.rng.GetBytes(this.buffer);
                    }

                    sample = BitConverter.ToInt32(this.buffer, offset);

                    // range of sample is [Int32.MinValue, Int32.MaxValue].
                    // we need [0, Int32.MaxValue - rerollThreshold).
                    // for the left half, we can just mask away the first bit.
                    // it's silly to discard a perfectly good random bit,
                    // but it's even sillier to bend over backwards to save it.
                    sample &= 0x7FFFFFFF;
                    offset = (offset + 4) % BufferSizeInBytes;
                }
                while (Int32.MaxValue - rerollThreshold < sample);

                this.nextOffset = offset;
            }

            sample %= rangeSize;
            return minValueInclusive + sample;
        }

        protected override void DisposeCore()
        {
            this.rng.Dispose();
        }
    }
}
