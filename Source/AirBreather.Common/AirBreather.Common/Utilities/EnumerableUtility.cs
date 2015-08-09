using System;
using System.Collections.Generic;
using System.Linq;

namespace AirBreather.Common.Utilities
{
    public static class EnumerableUtility
    {
        public static IEnumerable<uint> Range(uint start, uint count)
        {
            for (uint end = start + count; start < end; start++)
            {
                yield return start;
            }
        }

        // lets me write this:
        //     someEnumerable.ExceptWhere(someSet.Contains)
        // instead of this:
        //     someEnumerable.Where(x => !someSet.Contains(x))
        // and this also slightly improves fluent readability
        public static IEnumerable<T> ExceptWhere<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            // LINQ's .Where() has lots of optimizations,
            // so use that instead of re-implementing it.
            return enumerable.Where(x => !predicate(x));
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable) => ToHashSet(enumerable, null);
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable, IEqualityComparer<T> equalityComparer)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            return new HashSet<T>(enumerable, equalityComparer);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TCompare>(this IEnumerable<TSource> enumerable, Func<TSource, TCompare> selector) => DistinctBy(enumerable, selector, null);
        public static IEnumerable<TSource> DistinctBy<TSource, TCompare>(this IEnumerable<TSource> enumerable, Func<TSource, TCompare> selector, IEqualityComparer<TCompare> equalityComparer)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return DistinctByIterator(enumerable, selector, equalityComparer);
        }

        private static IEnumerable<TSource> DistinctByIterator<TSource, TCompare>(IEnumerable<TSource> enumerable, Func<TSource, TCompare> selector, IEqualityComparer<TCompare> equalityComparer)
        {
            HashSet<TCompare> closedSet = new HashSet<TCompare>(equalityComparer);
            foreach (TSource value in enumerable)
            {
                if (closedSet.Add(selector(value)))
                {
                    yield return value;
                }
            }
        }

        public static void CopyTo<T>(this IEnumerable<T> enumerable, T[] array, int arrayIndex)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "Must be non-negative.");
            }

            if (array.Length <= arrayIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "Must be less than the length of the array.");
            }

            foreach (T item in enumerable)
            {
                if (arrayIndex == array.Length)
                {
                    throw new ArgumentException("Not enough room", nameof(array));
                }

                array[arrayIndex++] = item;
            }
        }
    }
}
