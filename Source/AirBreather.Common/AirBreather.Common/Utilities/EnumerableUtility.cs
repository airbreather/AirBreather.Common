using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using AirBreather.Common.Collections;

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

        // these exist for the same reason as AsEnumerable<T>(), though their use case is weaker.
        public static IReadOnlyCollection<T> AsReadOnlyCollection<T>(this IReadOnlyCollection<T> collection) => collection;
        public static IReadOnlyList<T> AsReadOnlyList<T>(this IReadOnlyList<T> list) => list;
        public static IReadOnlyDictionary<TKey, TValue> AsReadOnlyDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary) => dictionary;
        public static IReadOnlySet<T> AsReadOnlySet<T>(this IReadOnlySet<T> set) => set;

        // these exist for the same reason as List<T>.AsReadOnly().
        public static ReadOnlyCollection<T> AsReadOnly<T>(this IList<T> list) => new ReadOnlyCollection<T>(list.ValidateNotNull(nameof(list)));
        public static ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) => new ReadOnlyDictionary<TKey, TValue>(dictionary.ValidateNotNull(nameof(dictionary)));
        public static ReadOnlySet<T> AsReadOnly<T>(this ISet<T> set) => new ReadOnlySet<T>(set.ValidateNotNull(nameof(set)));

        // these are a new idea that the BCL team didn't bother doing: a way to use the IReadOnlyFoo
        // types added in .NET 4.5 in APIs that accept what I'll term "soft read-only" collections,
        // i.e., their IFoo counterparts for which IsReadOnly is true.
        public static ReadOnlyCollectionWrapper<T> AsSoftReadOnly<T>(this IReadOnlyCollection<T> collection) => new ReadOnlyCollectionWrapper<T>(collection.ValidateNotNull(nameof(collection)));
        public static ReadOnlyListWrapper<T> AsSoftReadOnly<T>(this IReadOnlyList<T> list) => new ReadOnlyListWrapper<T>(list.ValidateNotNull(nameof(list)));
        public static ReadOnlyDictionaryWrapper<TKey, TValue> AsSoftReadOnly<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary) => new ReadOnlyDictionaryWrapper<TKey, TValue>(dictionary.ValidateNotNull(nameof(dictionary)));
        public static ReadOnlySetWrapper<T> AsSoftReadOnly<T>(this IReadOnlySet<T> set) => new ReadOnlySetWrapper<T>(set.ValidateNotNull(nameof(set)));

        // I was a little surprised not to see DistinctBy.  My only guess as to why they didn't is
        // because the use cases it supports aren't particularly compelling.  Well... we found at
        // least one or two at work, and I have to say... I agree, there are better designs.  BUT,
        // for various internal team reasons, this would have been the overall least-bad solution.
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

            int? countProperty = enumerable.GetCountPropertyIfAvailable();

            bool prevalidate = countProperty.HasValue;
            if (prevalidate)
            {
                if (array.Length - arrayIndex < countProperty.Value)
                {
                    throw new ArgumentException("Not enough room", nameof(array));
                }
            }

            foreach (T item in enumerable)
            {
                if (!prevalidate && arrayIndex == array.Length)
                {
                    throw new ArgumentException("Not enough room", nameof(array));
                }

                array[arrayIndex++] = item;
            }
        }

        // A version of the above that does slightly less work for IReadOnlyCollection<T> instances.
        // Should JIT to something that gets optimized to something that's only marginally different
        // than the other one, but it's still cheap and easy to do it this way.
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

        // Gets the Count property, if one is present on the type.
        // Checks interfaces it could implement, in what I expect to be the optimal order.
        public static int? GetCountPropertyIfAvailable<T>(this IEnumerable<T> enumerable)
        {
            // I think ICollection<T> is actually more likely than IReadOnlyCollection<T>, despite
            // the former requiring more work to implement on top of IEnumerable<T> than the latter,
            // because it's been around for several more years.  Furthermore, the fact that LINQ-to-
            // Objects itself uses this and not IReadOnlyCollection<T> for its own optimizations is
            // a pretty good incentive for someone to implement ICollection<T> when they can.
            ICollection<T> collection = enumerable.ValidateNotNull(nameof(enumerable)) as ICollection<T>;
            if (collection != null)
            {
                return collection.Count;
            }

            // OK... so why ICollection next?  Seems more likely for someone to use one of the many
            // types that implement ICollection but not ICollection<T> or IReadOnlyCollection<T>
            // than it is for someone to be using a type that implements IReadOnlyCollection<T> but
            // not ICollection<T> or ICollection, AND even *notice* the difference in the ordering
            // of these calls (more likely, they've got a user-defined collection type that's got
            // even slightly sub-optimal client code that, if optimized, would do literally millions
            // of times better than switching this order).
            System.Collections.ICollection legacyCollection = enumerable as System.Collections.ICollection;
            if (legacyCollection != null)
            {
                return legacyCollection.Count;
            }

            IReadOnlyCollection<T> readOnlyCollection = enumerable as IReadOnlyCollection<T>;
            if (readOnlyCollection != null)
            {
                return readOnlyCollection.Count;
            }

            // We've exhausted all the collection-with-Count-property interfaces that I care to do.
            return default(int?);
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, params T[] values) => Enumerable.Concat(enumerable, values);
    }
}
