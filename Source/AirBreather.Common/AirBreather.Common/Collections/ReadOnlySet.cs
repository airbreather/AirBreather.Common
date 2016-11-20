using System;
using System.Collections.Generic;

namespace AirBreather.Collections
{
    public class ReadOnlySet<T> : IReadOnlySet<T>, ISet<T>
    {
        private readonly ISet<T> set;

        public ReadOnlySet(ISet<T> set) => this.set = set.ValidateNotNull(nameof(set));

        public bool IsReadOnly => true;
        public int Count => this.set.Count;

        public bool IsSubsetOf(IEnumerable<T> other) => this.set.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<T> other) => this.set.IsSupersetOf(other);
        public bool IsProperSupersetOf(IEnumerable<T> other) => this.set.IsProperSupersetOf(other);
        public bool IsProperSubsetOf(IEnumerable<T> other) => this.set.IsProperSubsetOf(other);
        public bool Overlaps(IEnumerable<T> other) => this.set.Overlaps(other);
        public bool SetEquals(IEnumerable<T> other) => this.set.SetEquals(other);
        public bool Contains(T item) => this.set.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => this.set.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => this.set.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

        #region Unsupported (Read-Only)

        bool ISet<T>.Add(T item) => throw new NotSupportedException("Set is read-only.");
        void ICollection<T>.Add(T item) => throw new NotSupportedException("Set is read-only.");
        void ISet<T>.UnionWith(IEnumerable<T> other) => throw new NotSupportedException("Set is read-only.");
        void ISet<T>.IntersectWith(IEnumerable<T> other) => throw new NotSupportedException("Set is read-only.");
        void ISet<T>.ExceptWith(IEnumerable<T> other) => throw new NotSupportedException("Set is read-only.");
        void ISet<T>.SymmetricExceptWith(IEnumerable<T> other) => throw new NotSupportedException("Set is read-only.");
        void ICollection<T>.Clear() => throw new NotSupportedException("Set is read-only.");
        bool ICollection<T>.Remove(T item) => throw new NotSupportedException("Set is read-only.");

        #endregion
    }
}

