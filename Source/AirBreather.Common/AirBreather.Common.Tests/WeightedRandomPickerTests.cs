using System.Collections.Generic;
using System.Linq;

using Xunit;
using Xunit.Abstractions;

using AirBreather.Random;

namespace AirBreather.Tests
{
    public sealed class WeightedRandomPickerTests
    {
        private readonly ITestOutputHelper output;

        public WeightedRandomPickerTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TestWeightedRandomPicker()
        {
            var builder = new WeightedRandomPicker<string>.Builder();

            Dictionary<string, int> valsWithWeights = new Dictionary<string, int>
            {
                ["orly"] = 2,
                ["yarly"] = 1,
                ["nowai"] = 77,
                ["yaweh"] = 2
            };

            foreach (var kvp in valsWithWeights)
            {
                builder = builder.AddWithWeight(kvp.Key, kvp.Value);
            }

            var picker = builder.Build();

            var dct = valsWithWeights.Keys.ToDictionary(x => x, x => 0);

            var rnd = new System.Random(new CryptographicRandomGenerator().NextInt32());
            const int TrialCount = 100000;
            for (int i = 0; i < TrialCount; i++)
            {
                dct[picker.Pick(rnd.NextDouble())]++;
            }

            double scalar = TrialCount / (double)valsWithWeights.Values.Sum();
            var expect = valsWithWeights.ToDictionary(kvp => kvp.Key, kvp => kvp.Value * scalar);

            foreach (var kvp in dct)
            {
                this.output.WriteLine("{0} appeared {1} times (expected {2})", kvp.Key, kvp.Value, expect[kvp.Key]);
            }
        }
    }
}
