using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

using AirBreather.Common.Utilities;

namespace AirBreather.Common
{
    public static class Boxes
    {
        private static readonly object TrueBox = true;
        private static readonly object FalseBox = false;

        private const int IntBoxMaxExclusive = 11;
        private static readonly object[] IntBoxes =
        {
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10
        };

        private const uint UIntBoxMaxExclusive = 11;
        private static readonly object[] UIntBoxes =
        {
            0u,
            1u,
            2u,
            3u,
            4u,
            5u,
            6u,
            7u,
            8u,
            9u,
            10u
        };

        private static readonly Dictionary<double, object> DoubleBoxes;
        private static readonly Dictionary<float, object> SingleBoxes;

        static Boxes()
        {
            double[] someDoubleValues =
            {
                0.1,
                0.2,
                0.25,
                0.3,
                1 / 3d,
                0.4,
                0.5,
                0.6,
                2 / 3d,
                0.75,
                1,
                1.5,
                2,
                2.5,
                3,
                4,
                5,
                10,
                45,
                90,
                100,
                180,
                270,
                360,
                1000,
                10000,
                int.MaxValue,
                uint.MaxValue,
                Stopwatch.Frequency,
                Math.PI * 0.25,
                Math.PI * 0.5,
                Math.PI * 0.75,
                Math.PI * 1,
                Math.PI * 1.25,
                Math.PI * 1.5,
                Math.PI * 1.75,
                Math.PI * 2,
                Math.E,
                Math.Sqrt(2)
            };

            DoubleBoxes = someDoubleValues.Concat(Array.ConvertAll(someDoubleValues, v => -v)).Concat(0, double.MinValue, double.MaxValue, double.PositiveInfinity, double.NegativeInfinity, double.Epsilon).ToDictionary(val => val, val => (object)val);
            SingleBoxes = someDoubleValues.Concat(Array.ConvertAll(someDoubleValues, v => -v)).Select(Convert.ToSingle).Concat(0, float.MinValue, float.MaxValue, float.PositiveInfinity, float.NegativeInfinity, float.Epsilon).ToDictionary(val => val, val => (object)val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Boolean(bool value) => value ? TrueBox : FalseBox;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Int32(int value) => value.IsInRange(0, IntBoxMaxExclusive) ? IntBoxes[value] : value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object UInt32(uint value) => value.IsInRange(0u, UIntBoxMaxExclusive) ? UIntBoxes[value] : value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Double(double value)
        {
            object result;
            return DoubleBoxes.TryGetValue(value, out result) ? result : value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Single(float value)
        {
            object result;
            return SingleBoxes.TryGetValue(value, out result) ? result : value;
        }
    }
}
