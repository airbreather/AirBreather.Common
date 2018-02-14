using System;
using System.Runtime.CompilerServices;

namespace AirBreather
{
    public static partial class ComparableUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange<T>(this T value, T minInclusive, T maxExclusive) where T : IComparable<T> => minInclusive.CompareTo(value) <= 0 && value.CompareTo(maxExclusive) < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotLessThan<T>(this T value, T minInclusive) where T : IComparable<T> => minInclusive.CompareTo(value) <= 0;
    }
}
