using System;

using BclRandom = System.Random;

namespace AirBreather.Core.Random
{
    // Note that the BCL random provider is slightly biased.
    // It's extremely easy to use, which is why this exists,
    // but I won't be satisfied if this is the only pseudorandom
    // provider that exists.
    public sealed class BclPseudorandomPicker : IPicker
    {
        private readonly BclRandom random;

        private readonly object lockObject = new object();

        // Unlike System.Random, a seed MUST be provided.
        public BclPseudorandomPicker(int seed)
        {
            this.random = new BclRandom(seed);
        }

        public int PickFromRange(int minValueInclusive, int rangeSize)
        {
            // System.Random also permits 0, which is ridiculous
            // because it's a special-case that doesn't need to exist.
            if (rangeSize < 1)
            {
                throw new ArgumentOutOfRangeException("rangeSize", rangeSize, "Must be positive");
            }

            if (minValueInclusive > Int32.MaxValue - rangeSize)
            {
                throw new ArgumentOutOfRangeException("minValueInclusive", minValueInclusive, "Must be small enough to avoid overflow");
            }

            int pick;

            lock (this.lockObject)
            {
                // result is from [0, rangeSize)
                // therefore, there are rangeSize possible results.
                pick = this.random.Next(rangeSize);
            }

            return minValueInclusive + pick;
        }
    }
}
