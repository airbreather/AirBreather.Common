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
            private readonly object syncLock = new object();
            private List<WeightedItem> weightedItems = new List<WeightedItem>();

            public void AddWithWeight(T item, double weight)
            {
                if (Double.IsNaN(weight) || Double.IsInfinity(weight) || weight <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(weight), weight, "weight must be reasonable (and positive non-zero).  come on, be nice, I was about to make this int and everything... I'm doing you a favor by letting you use doubles, and this is how you repay me?  that's it, I give up.");
                }

                lock (this.syncLock)
                {
                    this.weightedItems.Add(new WeightedItem(item, weight));
                }
            }

            public WeightedRandomPicker<T> Build()
            {
                List<WeightedItem> finalWeightedItems;
                lock (this.syncLock)
                {
                    finalWeightedItems = this.weightedItems;
                    this.weightedItems = new List<WeightedItem>();
                }

                if (finalWeightedItems.Count == 0)
                {
                    throw new InvalidOperationException("Must have at least one item.");
                }

                // sort the weighted items in increasing order by weight.  if there's a *very* large
                // gap in weights, I *think* that sorting them like this improves how closely we'll
                // match the expected distribution for the low ones; if not, I apologize to your CPU
                // (but I'm pretty sure that at least it won't be *worse*).
                finalWeightedItems.Sort((first, second) => first.Weight.CompareTo(second.Weight));

                T[] items = new T[finalWeightedItems.Count];
                double[] newWeights = new double[items.Length];

                // reweight, step 1: set each weight to the sum of itself and all weights before it.
                double weightSoFar = 0;
                for (int i = 0; i < items.Length; i++)
                {
                    WeightedItem weightedItem = finalWeightedItems[i];
                    items[i] = weightedItem.Item;
                    newWeights[i] = weightSoFar += weightedItem.Weight;
                }

                // reweight, step 2: divide each weight by the sum total of all weights observed.
                // the final entry's weight, therefore, should be 1.
                for (int i = 0; i < newWeights.Length; i++)
                {
                    newWeights[i] /= weightSoFar;
                }

                Debug.Assert(newWeights[newWeights.Length - 1] == 1, "Any double value divided by itself should be 1...");
                newWeights[newWeights.Length - 1] = 1;

                return new WeightedRandomPicker<T>(items, newWeights);
            }
        }

        private sealed class WeightedItem
        {
            internal WeightedItem(T item, double weight)
            {
                this.Item = item;
                this.Weight = weight;
            }

            internal T Item { get; set; }
            internal double Weight { get; set; }
        }
    }
}
