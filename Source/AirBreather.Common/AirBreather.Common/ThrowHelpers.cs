using System;
using System.Runtime.CompilerServices;

using static System.FormattableString;

namespace AirBreather
{
    internal static class ThrowHelpers
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_Min<T>(string paramName, T value, T minInclusive) =>
            throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be greater than or equal to {minInclusive}."));

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_TwoBounds<T>(string paramName, T value, T minInclusive, T maxExclusive) =>
            throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be between [{minInclusive}, {maxExclusive})."));

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentNullException(string paramName) => throw new ArgumentNullException(paramName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException(string paramName) => throw new ArgumentException(paramName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException(string message, string paramName) => throw new ArgumentException(message, paramName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowExceptionForNonBlittable() => throw new ArgumentException("Generic type parameter must not contain any managed references.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentExceptionForAllZeroes() => throw new ArgumentException("At least one seed value must be non-zero.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentExceptionForBadTargetSpanLength() => throw new ArgumentException("Length of the destination span must be exactly twice the length of the source span (two output chars per input byte).");
    }
}
