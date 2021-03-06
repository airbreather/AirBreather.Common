﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace AirBreather.Collections
{
    public static class HashSetValue
    {
        public static HashSetValue<T> Create<T>(IEnumerable<T> values) => new HashSetValue<T>(values);

        public static bool Equals<T>(HashSetValue<T> first, HashSetValue<T> second) => first.SetEquals(second);

        public static int GetHashCode<T>(HashSetValue<T> value)
        {
            ImmutableHashSet<T> underlyingSet = value.UnderlyingSet;

            int dataHashCode = 0;
            foreach (T element in underlyingSet)
            {
                // XOR, because the ordering must not matter.
                dataHashCode ^= EqualityComparer<T>.Default.GetHashCode(element);
            }

            return HashCode.Seed
                           .HashWith(underlyingSet.Count)
                           .HashWith(dataHashCode);
        }
    }

    public readonly struct HashSetValue<T> : IReadOnlySet<T>, IImmutableSet<T>, IEquatable<HashSetValue<T>>
    {
        private readonly ImmutableHashSet<T> values;

        public HashSetValue(IEnumerable<T> values) => this.values = values.ValidateNotNull(nameof(values)).ToImmutableHashSet(EqualityComparer<T>.Default);

        public ImmutableHashSet<T> UnderlyingSet => this.values ?? ImmutableHashSet<T>.Empty;
        public int Count => this.UnderlyingSet.Count;

        public bool Contains(T item) => this.UnderlyingSet.Contains(item);
        public bool TryGetValue(T equalValue, out T actualValue) => this.UnderlyingSet.TryGetValue(equalValue, out actualValue);
        public bool IsProperSubsetOf(IEnumerable<T> other) => this.UnderlyingSet.IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<T> other) => this.UnderlyingSet.IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<T> other) => this.UnderlyingSet.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<T> other) => this.UnderlyingSet.IsSupersetOf(other);
        public bool Overlaps(IEnumerable<T> other) => this.UnderlyingSet.Overlaps(other);
        public bool SetEquals(IEnumerable<T> other) => this.UnderlyingSet.SetEquals(other);

        public HashSetValue<T> Clear() => new HashSetValue<T>(this.UnderlyingSet.Clear());
        public HashSetValue<T> Add(T value) => new HashSetValue<T>(this.UnderlyingSet.Add(value));
        public HashSetValue<T> Remove(T value) => new HashSetValue<T>(this.UnderlyingSet.Remove(value));
        public HashSetValue<T> Intersect(IEnumerable<T> other) => new HashSetValue<T>(this.UnderlyingSet.Intersect(other));
        public HashSetValue<T> Except(IEnumerable<T> other) => new HashSetValue<T>(this.UnderlyingSet.Except(other));
        public HashSetValue<T> SymmetricExcept(IEnumerable<T> other) => new HashSetValue<T>(this.UnderlyingSet.SymmetricExcept(other));
        public HashSetValue<T> Union(IEnumerable<T> other) => new HashSetValue<T>(this.UnderlyingSet.Union(other));

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

        public static bool operator ==(HashSetValue<T> first, HashSetValue<T> second) => HashSetValue.Equals(first, second);
        public static bool operator !=(HashSetValue<T> first, HashSetValue<T> second) => !HashSetValue.Equals(first, second);
        public override bool Equals(object obj) => obj is HashSetValue<T> && HashSetValue.Equals(this, (HashSetValue<T>)obj);
        public bool Equals(HashSetValue<T> other) => HashSetValue.Equals(this, other);
        public override int GetHashCode() => HashSetValue.GetHashCode(this);
    }
}
