using System;

using BclRandom = System.Random;

namespace AirBreather.Random
{
    // Note that the BCL random provider is slightly biased.
    // It's extremely easy to use, which is why this exists,
    // but I won't be satisfied if this is the only pseudorandom
    // provider that exists.
    public sealed class BclPseudorandomPicker : IPicker
    {
        private readonly BclRandom random;

        private readonly object lockObject = new object();

        // Unlike System.Random, a seed MUST be provided explicitly.
        public BclPseudorandomPicker(int seed) => this.random = new BclRandom(seed);

        public int PickFromRange(int minValueInclusive, int rangeSize)
        {
            // System.Random also permits 0, which is ridiculous
            // because it's a special-case that doesn't need to exist.
            rangeSize.ValidateNotLessThan(nameof(rangeSize), 1);
            (int.MaxValue - rangeSize).ValidateNotLessThan(nameof(minValueInclusive), minValueInclusive);

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
