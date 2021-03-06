﻿using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

using static AirBreather.ThrowHelpers;

namespace AirBreather
{
    public static partial class ValidationUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ValidateInRange<T>(this T value, string paramName, T minInclusive, T maxExclusive)
            where T : IComparable<T>
        {
            if (!value.IsInRange(minInclusive, maxExclusive))
            {
                ThrowArgumentOutOfRangeException_TwoBounds(paramName, value, minInclusive, maxExclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ValidateNotLessThan<T>(this T value, string paramName, T minInclusive)
            where T : IComparable<T>
        {
            if (!value.IsNotLessThan(minInclusive))
            {
                ThrowArgumentOutOfRangeException_Min(paramName, value, minInclusive);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ValidateNotNull<T>(this T value, string paramName) where T : class
        {
            if (value == null)
            {
                ThrowArgumentNullException(paramName);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImmutableArray<T> ValidateNotDefault<T>(this ImmutableArray<T> value, string paramName)
        {
            if (value.IsDefault)
            {
                ThrowArgumentNullException(paramName);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ValidateNotNullOrEmpty(this string value, string paramName)
        {
            // if it's null, we want ArgumentNullException instead.
            if (value.ValidateNotNull(paramName).Length == 0)
            {
                ThrowArgumentException("Must be non-empty", paramName);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ValidateNotBlank(this string value, string paramName)
        {
            if (value.ValidateNotNull(paramName).IsWhiteSpace())
            {
                ThrowArgumentException("Must be non-blank.", paramName);
            }

            return value;
        }

        private static bool IsWhiteSpace(this string value)
        {
            foreach (char ch in value)
            {
                if (!char.IsWhiteSpace(ch))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
