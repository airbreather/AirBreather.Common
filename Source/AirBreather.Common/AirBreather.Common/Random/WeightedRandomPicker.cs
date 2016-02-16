using System;
using System.Collections.Generic;
using System.Diagnostics;

using AirBreather.Common.Utilities;

namespace AirBreather.Common.Random
{
    public sealed class WeightedRandomPicker<T>
    {
        private readonly T[] items;
        private readonly double[] weights;

        private WeightedRandomPicker(T[] items, double[] weights)
        {
            this.items = items;
            this.weights = weights;
        }

        public T Pick(double roll)
        {
            roll.ValidateInRange(nameof(roll), 0, 1);
            int idx = Array.BinarySearch(this.weights, roll);
            if (idx < 0)
            {
                idx = ~idx;
            }

            return this.items[idx];
        }

        public sealed class Builder
        {
            private readonly List<T> items = new List<T>();
            private readonly List<double> weights = new List<double>();

            public void AddWithWeight(T item, double weight)
            {
                if (Double.IsNaN(weight) || Double.IsInfinity(weight) || weight <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(weight), weight, "weight must be reasonable (and positive non-zero).  come on, be nice, I was about to make this int and everything... I'm doing you a favor by letting you use doubles, and this is how you repay me?  that's it, I give up.");
                }

                this.items.Add(item);
                this.weights.Add(weight.ValidateNotLessThan(nameof(weight), 0));
            }

            public WeightedRandomPicker<T> Build()
            {
                if (this.items.Count == 0)
                {
                    throw new InvalidOperationException("Must have at least one item.");
                }

                double[] newWeights = new double[this.weights.Count];

                // reweight, step 1: set each weight to the sum of itself and all weights before it.
                double weightSoFar = 0;
                for (int i = 0; i < newWeights.Length; i++)
                {
                    weightSoFar += this.weights[i];
                    newWeights[i] = weightSoFar;
                }

                // reweight, step 2: divide each weight by the sum total of all weights observed.
                // the final entry's weight, therefore, should be 1.
                for (int i = 0; i < newWeights.Length; i++)
                {
                    newWeights[i] /= weightSoFar;
                }

                Debug.Assert(newWeights[newWeights.Length - 1] == 1, "Any double value divided by itself should be 1...");
                newWeights[newWeights.Length - 1] = 1;

                return new WeightedRandomPicker<T>(this.items.ToArray(), newWeights);
            }
        }
    }
}
