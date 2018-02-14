using System.Runtime.CompilerServices;

namespace AirBreather
{
    public static partial class ComparableUtility
    {
        #region IsInRange

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this double value, double minInclusive, double maxExclusive) => minInclusive <= value && value < maxExclusive;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this float value, float minInclusive, float maxExclusive) => minInclusive <= value && value < maxExclusive;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this long value, long minInclusive, long maxExclusive) => minInclusive <= value && value < maxExclusive;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this int value, int minInclusive, int maxExclusive) => minInclusive <= value && value < maxExclusive;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this short value, short minInclusive, short maxExclusive) => minInclusive <= value && value < maxExclusive;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this byte value, byte minInclusive, byte maxExclusive) => minInclusive <= value && value < maxExclusive;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this ulong value, ulong minInclusive, ulong maxExclusive) => minInclusive <= value && value < maxExclusive;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this uint value, uint minInclusive, uint maxExclusive) => minInclusive <= value && value < maxExclusive;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this ushort value, ushort minInclusive, ushort maxExclusive) => minInclusive <= value && value < maxExclusive;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this sbyte value, sbyte minInclusive, sbyte maxExclusive) => minInclusive <= value && value < maxExclusive;

        #endregion IsInRange

        #region IsNotLessThan

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotLessThan(this double value, double minInclusive) => minInclusive <= value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotLessThan(this float value, float minInclusive) => minInclusive <= value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotLessThan(this long value, long minInclusive) => minInclusive <= value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotLessThan(this int value, int minInclusive) => minInclusive <= value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotLessThan(this short value, short minInclusive) => minInclusive <= value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotLessThan(this byte value, byte minInclusive) => minInclusive <= value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotLessThan(this ulong value, ulong minInclusive) => minInclusive <= value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotLessThan(this uint value, uint minInclusive) => minInclusive <= value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotLessThan(this ushort value, ushort minInclusive) => minInclusive <= value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotLessThan(this sbyte value, sbyte minInclusive) => minInclusive <= value;

        #endregion IsNotLessThan
    }
}
