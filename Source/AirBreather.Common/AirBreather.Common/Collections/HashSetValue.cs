using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

using AirBreather.Common.Utilities;

namespace AirBreather.Common.Collections
{
    public struct HashSetValue<T> : IReadOnlySet<T>, IImmutableSet<T>, IEquatable<HashSetValue<T>>
    {
        private ImmutableHashSet<T> values;

        public HashSetValue(IEnumerable<T> values)
        {
            values.ValidateNotNull(nameof(values));
            this.values = values.ToImmutableHashSet();
        }

        public HashSetValue(IEnumerable<T> values, IEqualityComparer<T> comparer)
        {
            values.ValidateNotNull(nameof(values));
            this.values = values.ToImmutableHashSet(comparer);
        }

        public ImmutableHashSet<T> UnderlyingSet => this.values ?? ImmutableHashSet<T>.Empty;
        public IEqualityComparer<T> KeyComparer => this.UnderlyingSet.KeyComparer;
        public int Count => this.UnderlyingSet.Count;

        public bool Contains(T item) => this.UnderlyingSet.Contains(item);
        public bool TryGetValue(T equalValue, out T actualValue) => this.UnderlyingSet.TryGetValue(equalValue, out actualValue);
        public bool IsProperSubsetOf(IEnumerable<T> other) => this.UnderlyingSet.IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<T> other) => this.UnderlyingSet.IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<T> other) => this.UnderlyingSet.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<T> other) => this.UnderlyingSet.IsSupersetOf(other);
        public bool Overlaps(IEnumerable<T> other) => this.UnderlyingSet.Overlaps(other);
        public bool SetEquals(IEnumerable<T> other) => this.UnderlyingSet.SetEquals(other);

        public HashSetValue<T> Clear() => new HashSetValue<T>(this.UnderlyingSet.Clear(), this.UnderlyingSet.KeyComparer);
        public HashSetValue<T> Add(T value) => new HashSetValue<T>(this.UnderlyingSet.Add(value), this.UnderlyingSet.KeyComparer);
        public HashSetValue<T> Remove(T value) => new HashSetValue<T>(this.UnderlyingSet.Remove(value), this.UnderlyingSet.KeyComparer);
        public HashSetValue<T> Intersect(IEnumerable<T> other) => new HashSetValue<T>(this.UnderlyingSet.Intersect(other), this.UnderlyingSet.KeyComparer);
        public HashSetValue<T> Except(IEnumerable<T> other) => new HashSetValue<T>(this.UnderlyingSet.Except(other), this.UnderlyingSet.KeyComparer);
        public HashSetValue<T> SymmetricExcept(IEnumerable<T> other) => new HashSetValue<T>(this.UnderlyingSet.SymmetricExcept(other), this.UnderlyingSet.KeyComparer);
        public HashSetValue<T> Union(IEnumerable<T> other) => new HashSetValue<T>(this.UnderlyingSet.Union(other), this.UnderlyingSet.KeyComparer);

        IImmutableSet<T> IImmutableSet<T>.Clear() => this.Clear();
        IImmutableSet<T> IImmutableSet<T>.Add(T value) => this.Add(value);
        IImmutableSet<T> IImmutableSet<T>.Remove(T value) => this.Remove(value);
        IImmutableSet<T> IImmutableSet<T>.Intersect(IEnumerable<T> other) => this.Intersect(other);
        IImmutableSet<T> IImmutableSet<T>.Except(IEnumerable<T> other) => this.Except(other);
        IImmutableSet<T> IImmutableSet<T>.SymmetricExcept(IEnumerable<T> other) => this.SymmetricExcept(other);
        IImmutableSet<T> IImmutableSet<T>.Union(IEnumerable<T> other) => this.Union(other);

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
