using System.Runtime.CompilerServices;

using static AirBreather.ThrowHelpers;

namespace AirBreather
{
    public static partial class ValidationUtility
    {
        #region ValidateInRange

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ValidateInRange(this double value, string paramName, double minInclusive, double maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                ThrowArgumentOutOfRangeException_TwoBounds(paramName, value, minInclusive, maxExclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ValidateInRange(this float value, string paramName, float minInclusive, float maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                ThrowArgumentOutOfRangeException_TwoBounds(paramName, value, minInclusive, maxExclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ValidateInRange(this long value, string paramName, long minInclusive, long maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                ThrowArgumentOutOfRangeException_TwoBounds(paramName, value, minInclusive, maxExclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ValidateInRange(this int value, string paramName, int minInclusive, int maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                ThrowArgumentOutOfRangeException_TwoBounds(paramName, value, minInclusive, maxExclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ValidateInRange(this short value, string paramName, short minInclusive, short maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                ThrowArgumentOutOfRangeException_TwoBounds(paramName, value, minInclusive, maxExclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ValidateInRange(this byte value, string paramName, byte minInclusive, byte maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                ThrowArgumentOutOfRangeException_TwoBounds(paramName, value, minInclusive, maxExclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ValidateInRange(this ulong value, string paramName, ulong minInclusive, ulong maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                ThrowArgumentOutOfRangeException_TwoBounds(paramName, value, minInclusive, maxExclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ValidateInRange(this uint value, string paramName, uint minInclusive, uint maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                ThrowArgumentOutOfRangeException_TwoBounds(paramName, value, minInclusive, maxExclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ValidateInRange(this ushort value, string paramName, ushort minInclusive, ushort maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                ThrowArgumentOutOfRangeException_TwoBounds(paramName, value, minInclusive, maxExclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ValidateInRange(this sbyte value, string paramName, sbyte minInclusive, sbyte maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                ThrowArgumentOutOfRangeException_TwoBounds(paramName, value, minInclusive, maxExclusive);
            }

            return value;
        }

        #endregion ValidateInRange

        #region ValidateNotLessThan

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ValidateNotLessThan(this double value, string paramName, double minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                ThrowArgumentOutOfRangeException_Min(paramName, value, minInclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ValidateNotLessThan(this float value, string paramName, float minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                ThrowArgumentOutOfRangeException_Min(paramName, value, minInclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ValidateNotLessThan(this long value, string paramName, long minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                ThrowArgumentOutOfRangeException_Min(paramName, value, minInclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ValidateNotLessThan(this int value, string paramName, int minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                ThrowArgumentOutOfRangeException_Min(paramName, value, minInclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ValidateNotLessThan(this short value, string paramName, short minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                ThrowArgumentOutOfRangeException_Min(paramName, value, minInclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ValidateNotLessThan(this byte value, string paramName, byte minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                ThrowArgumentOutOfRangeException_Min(paramName, value, minInclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ValidateNotLessThan(this ulong value, string paramName, ulong minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                ThrowArgumentOutOfRangeException_Min(paramName, value, minInclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ValidateNotLessThan(this uint value, string paramName, uint minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                ThrowArgumentOutOfRangeException_Min(paramName, value, minInclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ValidateNotLessThan(this ushort value, string paramName, ushort minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                ThrowArgumentOutOfRangeException_Min(paramName, value, minInclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ValidateNotLessThan(this sbyte value, string paramName, sbyte minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                ThrowArgumentOutOfRangeException_Min(paramName, value, minInclusive);
            }

            return value;
        }

        #endregion ValidateNotLessThan
    }
}
