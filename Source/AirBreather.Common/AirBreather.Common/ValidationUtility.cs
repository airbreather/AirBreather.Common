using System;
using System.Collections.Immutable;

using static System.FormattableString;

namespace AirBreather
{
    public static class ValidationUtility
    {
        public static T ValidateInRange<T>(this T value, string paramName, T minInclusive, T maxExclusive) where T : IComparable<T> =>
            value.IsInRange(minInclusive, maxExclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be between [{minInclusive}, {maxExclusive})."));

        public static T ValidateNotLessThan<T>(this T value, string paramName, T minInclusive) where T : IComparable<T> =>
            value.IsNotLessThan(minInclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be greater than or equal to {minInclusive}."));

        // equivalents to the above, but for primitive types
        #region Primitives

        #region ValidateInRange

        public static double ValidateInRange(this double value, string paramName, double minInclusive, double maxExclusive) =>
            value.IsInRange(minInclusive, maxExclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be between [{minInclusive}, {maxExclusive})."));

        public static float ValidateInRange(this float value, string paramName, float minInclusive, float maxExclusive) =>
            value.IsInRange(minInclusive, maxExclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be between [{minInclusive}, {maxExclusive})."));

        public static long ValidateInRange(this long value, string paramName, long minInclusive, long maxExclusive) =>
            value.IsInRange(minInclusive, maxExclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be between [{minInclusive}, {maxExclusive})."));

        public static int ValidateInRange(this int value, string paramName, int minInclusive, int maxExclusive) =>
            value.IsInRange(minInclusive, maxExclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be between [{minInclusive}, {maxExclusive})."));

        public static short ValidateInRange(this short value, string paramName, short minInclusive, short maxExclusive) =>
            value.IsInRange(minInclusive, maxExclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be between [{minInclusive}, {maxExclusive})."));

        public static byte ValidateInRange(this byte value, string paramName, byte minInclusive, byte maxExclusive) =>
            value.IsInRange(minInclusive, maxExclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be between [{minInclusive}, {maxExclusive})."));

        public static ulong ValidateInRange(this ulong value, string paramName, ulong minInclusive, ulong maxExclusive) =>
            value.IsInRange(minInclusive, maxExclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be between [{minInclusive}, {maxExclusive})."));

        public static uint ValidateInRange(this uint value, string paramName, uint minInclusive, uint maxExclusive) =>
            value.IsInRange(minInclusive, maxExclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be between [{minInclusive}, {maxExclusive})."));

        public static ushort ValidateInRange(this ushort value, string paramName, ushort minInclusive, ushort maxExclusive) =>
            value.IsInRange(minInclusive, maxExclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be between [{minInclusive}, {maxExclusive})."));

        public static sbyte ValidateInRange(this sbyte value, string paramName, sbyte minInclusive, sbyte maxExclusive) =>
            value.IsInRange(minInclusive, maxExclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be between [{minInclusive}, {maxExclusive})."));

        #endregion ValidateInRange

        #region ValidateNotLessThan

        public static double ValidateNotLessThan(this double value, string paramName, double minInclusive) =>
            value.IsNotLessThan(minInclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be greater than or equal to {minInclusive}."));

        public static float ValidateNotLessThan(this float value, string paramName, float minInclusive) =>
            value.IsNotLessThan(minInclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be greater than or equal to {minInclusive}."));

        public static long ValidateNotLessThan(this long value, string paramName, long minInclusive) =>
            value.IsNotLessThan(minInclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be greater than or equal to {minInclusive}."));

        public static int ValidateNotLessThan(this int value, string paramName, int minInclusive) =>
            value.IsNotLessThan(minInclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be greater than or equal to {minInclusive}."));

        public static short ValidateNotLessThan(this short value, string paramName, short minInclusive) =>
            value.IsNotLessThan(minInclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be greater than or equal to {minInclusive}."));

        public static byte ValidateNotLessThan(this byte value, string paramName, byte minInclusive) =>
            value.IsNotLessThan(minInclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be greater than or equal to {minInclusive}."));

        public static ulong ValidateNotLessThan(this ulong value, string paramName, ulong minInclusive) =>
            value.IsNotLessThan(minInclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be greater than or equal to {minInclusive}."));

        public static uint ValidateNotLessThan(this uint value, string paramName, uint minInclusive) =>
            value.IsNotLessThan(minInclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be greater than or equal to {minInclusive}."));

        public static ushort ValidateNotLessThan(this ushort value, string paramName, ushort minInclusive) =>
            value.IsNotLessThan(minInclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be greater than or equal to {minInclusive}."));

        public static sbyte ValidateNotLessThan(this sbyte value, string paramName, sbyte minInclusive) =>
            value.IsNotLessThan(minInclusive)
                ? value
                : throw new ArgumentOutOfRangeException(paramName, value, Invariant($"Must be greater than or equal to {minInclusive}."));

        #endregion ValidateNotLessThan

        #endregion Primitives

        public static T ValidateNotNull<T>(this T value, string paramName) where T : class =>
            value ?? throw new ArgumentNullException(paramName);

        public static Span<T> ValidateNotDefault<T>(this Span<T> value, string paramName) =>
            value != default
                ? value
                : throw new ArgumentException(paramName);

        public static ReadOnlySpan<T> ValidateNotDefault<T>(this ReadOnlySpan<T> value, string paramName) =>
            value != default
                ? value
                : throw new ArgumentException(paramName);

        public static ImmutableArray<T> ValidateNotDefault<T>(this ImmutableArray<T> value, string paramName) =>
            !value.IsDefault
                ? value
                : throw new ArgumentNullException(paramName);

        // if it's null, we want ArgumentNullException instead.
        public static string ValidateNotNullOrEmpty(this string value, string paramName) =>
            value.ValidateNotNull(paramName).Length != 0
                ? value
                : throw new ArgumentException("Must be non-empty.", paramName);

        public static string ValidateNotBlank(this string value, string paramName) =>
            !String.IsNullOrWhiteSpace(value.ValidateNotNull(paramName))
                ? value
                : throw new ArgumentException("Must be non-blank.", paramName);
    }
}
