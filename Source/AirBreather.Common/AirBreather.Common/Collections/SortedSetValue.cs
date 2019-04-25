using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace AirBreather.Collections
{
    public static class SortedSetValue
    {
        public static SortedSetValue<T> Create<T>(IEnumerable<T> values) => new SortedSetValue<T>(values);

        public static bool Equals<T>(SortedSetValue<T> first, SortedSetValue<T> second) => first.SetEquals(second);

        public static int GetHashCode<T>(SortedSetValue<T> value)
        {
            int hashCode = HashCode.Seed;

            hashCode = hashCode.HashWith(value.Count);

            foreach (T element in value)
            {
                // Since SortedSet is sorted, we can make a better hash code than XOR.
                hashCode = hashCode.HashWith(element);
            }

            return hashCode;
        }
    }

    public struct SortedSetValue<T> : IReadOnlySet<T>, IImmutableSet<T>, IEquatable<SortedSetValue<T>>
    {
#pragma warning disable IDE0044
        private ImmutableSortedSet<T> values;
#pragma warning restore IDE0044

        public SortedSetValue(IEnumerable<T> values) => this.values = values.ValidateNotNull(nameof(values)).ToImmutableSortedSet(Comparer<T>.Default);

        public ImmutableSortedSet<T> UnderlyingSet => this.values ?? ImmutableSortedSet<T>.Empty;
        public int Count => this.UnderlyingSet.Count;

        public bool Contains(T item) => this.UnderlyingSet.Contains(item);
        public bool TryGetValue(T equalValue, out T actualValue) => this.UnderlyingSet.TryGetValue(equalValue, out actualValue);
        public bool IsProperSubsetOf(IEnumerable<T> other) => this.UnderlyingSet.IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<T> other) => this.UnderlyingSet.IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<T> other) => this.UnderlyingSet.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<T> other) => this.UnderlyingSet.IsSupersetOf(other);
        public bool Overlaps(IEnumerable<T> other) => this.UnderlyingSet.Overlaps(other);
        public bool SetEquals(IEnumerable<T> other) => this.UnderlyingSet.SetEquals(other);

        public SortedSetValue<T> Clear() => new SortedSetValue<T>(this.UnderlyingSet.Clear());
        public SortedSetValue<T> Add(T value) => new SortedSetValue<T>(this.UnderlyingSet.Add(value));
        public SortedSetValue<T> Remove(T value) => new SortedSetValue<T>(this.UnderlyingSet.Remove(value));
        public SortedSetValue<T> Intersect(IEnumerable<T> other) => new SortedSetValue<T>(this.UnderlyingSet.Intersect(other));
        public SortedSetValue<T> Except(IEnumerable<T> other) => new SortedSetValue<T>(this.UnderlyingSet.Except(other));
        public SortedSetValue<T> SymmetricExcept(IEnumerable<T> other) => new SortedSetValue<T>(this.UnderlyingSet.SymmetricExcept(other));
        public SortedSetValue<T> Union(IEnumerable<T> other) => new SortedSetValue<T>(this.UnderlyingSet.Union(other));

        IImmutableSet<T> IImmutableSet<T>.Clear() => this.Clear();
        IImmutableSet<T> IImmutableSet<T>.Add(T value) => this.Add(value);
        IImmutableSet<T> IImmutableSet<T>.Remove(T value) => this.Remove(value);
        IImmutableSet<T> IImmutableSet<T>.Intersect(IEnumerable<T> other) => this.Intersect(other);
        IImmutableSet<T> IImmutableSet<T>.Except(IEnumerable<T> other) => this.Except(other);
        IImmutableSet<T> IImmutableSet<T>.SymmetricExcept(IEnumerable<T> other) => this.SymmetricExcept(other);
        IImmutableSet<T> IImmutableSet<T>.Union(IEnumerable<T> other) => this.Union(other);

        public ImmutableSortedSet<T>.Enumerator GetEnumerator() => this.UnderlyingSet.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public static bool operator ==(SortedSetValue<T> first, SortedSetValue<T> second) => SortedSetValue.Equals(first, second);
        public static bool operator !=(SortedSetValue<T> first, SortedSetValue<T> second) => !SortedSetValue.Equals(first, second);
        public override bool Equals(object obj) => obj is SortedSetValue<T> && Equals(this, (SortedSetValue<T>)obj);
        public bool Equals(SortedSetValue<T> other) => SortedSetValue.Equals(this, other);
        public override int GetHashCode() => SortedSetValue.GetHashCode(this);
    }
}
