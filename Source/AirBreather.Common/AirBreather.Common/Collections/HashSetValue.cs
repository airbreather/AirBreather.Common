using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace AirBreather.Common.Collections
{
    public struct HashSetValue<T> : IReadOnlySet<T>, IEquatable<HashSetValue<T>>
    {
        private ImmutableHashSet<T> values;

        public HashSetValue(IEnumerable<T> values)
        {
            this.values = values.ToImmutableHashSet();
        }

        public HashSetValue(IEnumerable<T> values, IEqualityComparer<T> comparer)
        {
            this.values = values.ToImmutableHashSet(comparer);
        }

        public ImmutableHashSet<T> UnderlyingSet => this.values ?? ImmutableHashSet<T>.Empty;
        public IEqualityComparer<T> KeyComparer => this.UnderlyingSet.KeyComparer;
        public int Count => this.UnderlyingSet.Count;

        public bool Contains(T item) => this.UnderlyingSet.Contains(item);
        public bool IsProperSubsetOf(IEnumerable<T> other) => this.UnderlyingSet.IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<T> other) => this.UnderlyingSet.IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<T> other) => this.UnderlyingSet.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<T> other) => this.UnderlyingSet.IsSupersetOf(other);
        public bool Overlaps(IEnumerable<T> other) => this.UnderlyingSet.Overlaps(other);
        public bool SetEquals(IEnumerable<T> other) => this.UnderlyingSet.SetEquals(other);
        public ImmutableHashSet<T>.Enumerator GetEnumerator() => this.UnderlyingSet.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public static bool Equals(HashSetValue<T> first, HashSetValue<T> second) => first.SetEquals(second);
        public static bool operator ==(HashSetValue<T> first, HashSetValue<T> second) => Equals(first, second);
        public static bool operator !=(HashSetValue<T> first, HashSetValue<T> second) => !Equals(first, second);
        public override bool Equals(object obj) => obj is HashSetValue<T> && Equals(this, (HashSetValue<T>)obj);
        public bool Equals(HashSetValue<T> other) => Equals(this, other);
        public override int GetHashCode() => GetHashCode(this);
        public static int GetHashCode(HashSetValue<T> value)
        {
            ImmutableHashSet<T> underlyingSet = value.UnderlyingSet;

            int hashCode = underlyingSet.Count;
            foreach (T element in underlyingSet)
            {
                hashCode ^= underlyingSet.KeyComparer.GetHashCode(element);
            }

            return hashCode;
        }
    }
}
