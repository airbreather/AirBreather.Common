using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using AirBreather.Collections;
using AirBreather.Danger;

namespace AirBreather
{
    public static class EnumerableEx
    {
        public static IEnumerable<uint> Range(uint start, uint count)
        {
            for (uint end = start + count; start < end; ++start)
            {
                yield return start;
            }
        }

        public static IEnumerable<long> Range(long start, long count)
        {
            for (long end = start + count; start < end; ++start)
            {
                yield return start;
            }
        }

        public static IEnumerable<ulong> Range(ulong start, ulong count)
        {
            for (ulong end = start + count; start < end; ++start)
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

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> equalityComparer = null) => new Dictionary<TKey, TValue>(dictionary.ValidateNotNull(nameof(dictionary)), equalityComparer);

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable, IEqualityComparer<T> equalityComparer = null) => new HashSet<T>(enumerable.ValidateNotNull(nameof(enumerable)), equalityComparer);

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
        public static IEnumerable<TSource> DistinctBy<TSource, TCompare>(this IEnumerable<TSource> enumerable, Func<TSource, TCompare> selector, IEqualityComparer<TCompare> equalityComparer = null) => DistinctByIterator(enumerable.ValidateNotNull(nameof(enumerable)), selector.ValidateNotNull(nameof(selector)), equalityComparer);
        private static IEnumerable<TSource> DistinctByIterator<TSource, TCompare>(IEnumerable<TSource> enumerable, Func<TSource, TCompare> selector, IEqualityComparer<TCompare> equalityComparer)
        {
            var closedSet = new HashSet<TCompare>(equalityComparer);
            foreach (TSource value in enumerable)
            {
                if (closedSet.Add(selector(value)))
                {
                    yield return value;
                }
            }
        }

        public static void CopyTo<T>(this T[] src, Span<T> dst) => src.AsReadOnlySpan().CopyTo(dst);
        public static void CopyTo<T>(this ArraySegment<T> src, Span<T> dst) => src.AsReadOnlySpan().CopyTo(dst);
        public static void CopyTo<T>(this IEnumerable<T> enumerable, T[] array, int arrayIndex = 0)
        {
            enumerable.ValidateNotNull(nameof(enumerable));
            array.ValidateNotNull(nameof(array));
            arrayIndex.ValidateInRange(nameof(arrayIndex), 0, array.Length);

            bool prevalidate = enumerable.TryGetCount(out var count);
            if (prevalidate && array.Length - arrayIndex < count)
            {
                throw new ArgumentException("Not enough room", nameof(array));
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
        public static void CopyTo<T>(this IReadOnlyCollection<T> collection, T[] array, int arrayIndex = 0)
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
        public static bool TryGetCount<T>(this IEnumerable<T> enumerable, out int count)
        {
            switch (enumerable.ValidateNotNull(nameof(enumerable)))
            {
                // I think ICollection<T> is actually more likely than IReadOnlyCollection<T>,
                // despite the former requiring more work to implement on top of IEnumerable<T> than
                // the latter, because it's been around for several more years.  Furthermore, the
                // fact that LINQ-to-Objects itself uses this and not IReadOnlyCollection<T> for its
                // own optimizations is a good incentive for someone to implement ICollection<T>
                // when they can.
                case ICollection<T> collection:
                    count = collection.Count;
                    return true;

                // OK... so why ICollection next?  Seems more likely for someone to use one of the
                // types that implement ICollection but not ICollection<T> or IReadOnlyCollection<T>
                // than it is for someone to be using a type that implements IReadOnlyCollection<T>
                // but not ICollection<T> or ICollection, AND even *notice* the difference in the
                // ordering of these calls (more likely, they've got a user-defined collection type
                // that's got even slightly sub-optimal client code that, if optimized, would do
                // literally millions of times better than switching this order).
                case System.Collections.ICollection legacyCollection:
                    count = legacyCollection.Count;
                    return true;

                case IReadOnlyCollection<T> readOnlyCollection:
                    count = readOnlyCollection.Count;
                    return true;

                // We've exhausted all the collection-with-Count-property interfaces that I care to do.
                default:
                    count = 0;
                    return false;
            }
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, params T[] values) => Enumerable.Concat(enumerable, values);

        public static IEnumerable<T> Union<T>(this IEnumerable<T> enumerable, params T[] values) => Enumerable.Union(enumerable, values);

        public static ImmutableArray<T> MoveToImmutableSafe<T>(this ImmutableArray<T>.Builder builder)
        {
            // sometimes, this is equivalent to MoveToImmutable() with a negligible penalty.
            // otherwise, this is equivalent to a .ToImmutable() followed by a Clear().
            builder.Capacity = builder.Count;
            return builder.MoveToImmutable();
        }

        public static IEnumerable<(T value, int index)> TagIndexes<T>(this IEnumerable<T> source) => source.Select((value, index) => (value, index));

        public static IEnumerable<(T1 x1, T2 x2)> Zip<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second) => Enumerable.Zip(first, second, (x1, x2) => (x1, x2));

        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this ImmutableArray<T> array) => array.AsRegularArrayDangerous().AsReadOnlySpan();

        public static MemoryStream ToReadableStream(this ImmutableArray<byte> array, int index = 0) => ToReadableStreamCore(array, index, null);
        public static MemoryStream ToReadableStream(this ImmutableArray<byte> array, int index, int count) => ToReadableStreamCore(array, index, count);
        private static MemoryStream ToReadableStreamCore(ImmutableArray<byte> array, int index, int? count)
        {
            array.ValidateNotDefault(nameof(array));
            index.ValidateInRange(nameof(index), 0, array.Length);
            if (array.Length - index < count)
            {
                throw new ArgumentException("Not enough room", nameof(array));
            }

            return new MemoryStream(buffer: array.AsRegularArrayDangerous(), index: index, count: count ?? array.Length, writable: false, publiclyVisible: false);
        }

        public static ReadOnlySpanEnumerable<T> AsEnumerable<T>(this ReadOnlySpan<T> span) => new ReadOnlySpanEnumerable<T>(span);

        [StructLayout(LayoutKind.Auto)]
        public readonly ref struct ReadOnlySpanEnumerable<T>
        {
            private readonly ReadOnlySpan<T> span;

            internal ReadOnlySpanEnumerable(ReadOnlySpan<T> span) => this.span = span;

            public ReadOnlySpanEnumerator GetEnumerator() => new ReadOnlySpanEnumerator(this.span);

            [StructLayout(LayoutKind.Auto)]
            public ref struct ReadOnlySpanEnumerator
            {
                private ReadOnlySpan<T> span;

                private int idx;

                internal ReadOnlySpanEnumerator(ReadOnlySpan<T> span)
                {
                    this.span = span;
                    this.idx = -1;
                }

                public bool MoveNext() => this.idx < this.span.Length &&
                                          ++this.idx < this.span.Length;

                public T Current => this.span[this.idx];
            }
        }
    }
}
