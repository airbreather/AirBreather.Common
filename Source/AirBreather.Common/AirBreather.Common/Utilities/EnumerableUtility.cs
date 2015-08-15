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
            enumerable.ValidateNotNull(nameof(enumerable));
            predicate.ValidateNotNull(nameof(predicate));

            // LINQ's .Where() has lots of optimizations,
            // so use that instead of re-implementing it.
            return enumerable.Where(x => !predicate(x));
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable) => ToHashSet(enumerable, null);
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable, IEqualityComparer<T> equalityComparer) => new HashSet<T>(enumerable.ValidateNotNull(nameof(enumerable)), equalityComparer);

        public static IReadOnlyCollection<T> AsReadOnlyCollection<T>(this IReadOnlyCollection<T> collection) => collection;

        public static IEnumerable<TSource> DistinctBy<TSource, TCompare>(this IEnumerable<TSource> enumerable, Func<TSource, TCompare> selector) => DistinctBy(enumerable, selector, null);
        public static IEnumerable<TSource> DistinctBy<TSource, TCompare>(this IEnumerable<TSource> enumerable, Func<TSource, TCompare> selector, IEqualityComparer<TCompare> equalityComparer) => DistinctByIterator(enumerable.ValidateNotNull(nameof(enumerable)), selector.ValidateNotNull(nameof(selector)), equalityComparer);
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
            enumerable.ValidateNotNull(nameof(enumerable));
            array.ValidateNotNull(nameof(array));
            arrayIndex.ValidateInRange(nameof(arrayIndex), 0, array.Length);

            foreach (T item in enumerable)
            {
                if (arrayIndex == array.Length)
                {
                    throw new ArgumentException("Not enough room", nameof(array));
                }

                array[arrayIndex++] = item;
            }
        }

        // A version of the above that fails faster for IReadOnlyCollection<T> instances.
        public static void CopyTo<T>(this IReadOnlyCollection<T> collection, T[] array, int arrayIndex)
        {
            collection.ValidateNotNull(nameof(collection));
            array.ValidateNotNull(nameof(array));
            arrayIndex.ValidateInRange(nameof(arrayIndex), 0, array.Length);

            if (array.Length - arrayIndex < collection.Count)
            {
                throw new ArgumentException("Not enough room", nameof(array));
            }

            foreach (T item in collection)
            {
                array[arrayIndex++] = item;
            }
        }
    }
}
