using System;
using System.Globalization;

namespace AirBreather.Common.Utilities
{
    public static class ValidationUtility
    {
        public static T ValidateNotNull<T>(this T value, string paramName) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }

            return value;
        }

        public static T ValidateInRange<T>(this T value, string paramName, T minInclusive, T maxExclusive) where T : IComparable<T>
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be between [{0}, {1}).", minInclusive, maxExclusive));
            }

            return value;
        }

        public static T ValidateNotLessThan<T>(this T value, string paramName, T minInclusive) where T : IComparable<T>
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be greater than or equal to {0}.", minInclusive));
            }

            return value;
        }

        // equivalents to the above, but for primitive types
        #region Primitives

        #region ValidateInRange

        public static double ValidateInRange(this double value, string paramName, double minInclusive, double maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be between [{0}, {1}).", minInclusive, maxExclusive));
            }

            return value;
        }

        public static float ValidateInRange(this float value, string paramName, float minInclusive, float maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be between [{0}, {1}).", minInclusive, maxExclusive));
            }

            return value;
        }

        public static long ValidateInRange(this long value, string paramName, long minInclusive, long maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be between [{0}, {1}).", minInclusive, maxExclusive));
            }

            return value;
        }

        public static int ValidateInRange(this int value, string paramName, int minInclusive, int maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be between [{0}, {1}).", minInclusive, maxExclusive));
            }

            return value;
        }

        public static short ValidateInRange(this short value, string paramName, short minInclusive, short maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be between [{0}, {1}).", minInclusive, maxExclusive));
            }

            return value;
        }

        public static byte ValidateInRange(this byte value, string paramName, byte minInclusive, byte maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be between [{0}, {1}).", minInclusive, maxExclusive));
            }

            return value;
        }

        public static ulong ValidateInRange(this ulong value, string paramName, ulong minInclusive, ulong maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be between [{0}, {1}).", minInclusive, maxExclusive));
            }

            return value;
        }

        public static uint ValidateInRange(this uint value, string paramName, uint minInclusive, uint maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be between [{0}, {1}).", minInclusive, maxExclusive));
            }

            return value;
        }

        public static ushort ValidateInRange(this ushort value, string paramName, ushort minInclusive, ushort maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be between [{0}, {1}).", minInclusive, maxExclusive));
            }

            return value;
        }

        public static sbyte ValidateInRange(this sbyte value, string paramName, sbyte minInclusive, sbyte maxExclusive)
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be between [{0}, {1}).", minInclusive, maxExclusive));
            }

            return value;
        }

        #endregion ValidateInRange

        #region ValidateNotLessThan

        public static double ValidateNotLessThan(this double value, string paramName, double minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be greater than or equal to {0}.", minInclusive));
            }

            return value;
        }

        public static float ValidateNotLessThan(this float value, string paramName, float minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be greater than or equal to {0}.", minInclusive));
            }

            return value;
        }

        public static long ValidateNotLessThan(this long value, string paramName, long minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be greater than or equal to {0}.", minInclusive));
            }

            return value;
        }

        public static int ValidateNotLessThan(this int value, string paramName, int minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be greater than or equal to {0}.", minInclusive));
            }

            return value;
        }

        public static short ValidateNotLessThan(this short value, string paramName, short minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be greater than or equal to {0}.", minInclusive));
            }

            return value;
        }

        public static byte ValidateNotLessThan(this byte value, string paramName, byte minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be greater than or equal to {0}.", minInclusive));
            }

            return value;
        }

        public static ulong ValidateNotLessThan(this ulong value, string paramName, ulong minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be greater than or equal to {0}.", minInclusive));
            }

            return value;
        }

        public static uint ValidateNotLessThan(this uint value, string paramName, uint minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be greater than or equal to {0}.", minInclusive));
            }

            return value;
        }

        public static ushort ValidateNotLessThan(this ushort value, string paramName, ushort minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be greater than or equal to {0}.", minInclusive));
            }

            return value;
        }

        public static sbyte ValidateNotLessThan(this sbyte value, string paramName, sbyte minInclusive)
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                throw new ArgumentOutOfRangeException(paramName, value, String.Format(CultureInfo.InvariantCulture, "Must be greater than or equal to {0}.", minInclusive));
            }

            return value;
        }

        #endregion Primitives

        #endregion Primitives
    }
}

