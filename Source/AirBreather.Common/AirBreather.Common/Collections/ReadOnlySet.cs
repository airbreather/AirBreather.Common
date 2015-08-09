using System;
using System.Collections.Generic;

namespace AirBreather.Common.Collections
{
    public class ReadOnlySet<T> : IReadOnlySet<T>, ISet<T>
	{
		private readonly ISet<T> wrappedSet;

		public ReadOnlySet(ISet<T> wrappedSet)
		{
			if (wrappedSet == null)
			{
				throw new ArgumentNullException(nameof(wrappedSet));
			}

			this.wrappedSet = wrappedSet;
		}

        public bool IsReadOnly => true;
		public int Count => this.wrappedSet.Count;

		public bool IsSubsetOf(IEnumerable<T> other) => this.wrappedSet.IsSubsetOf(other);
		public bool IsSupersetOf(IEnumerable<T> other) => this.wrappedSet.IsSupersetOf(other);
		public bool IsProperSupersetOf(IEnumerable<T> other) => this.wrappedSet.IsProperSupersetOf(other);
		public bool IsProperSubsetOf(IEnumerable<T> other) => this.wrappedSet.IsProperSubsetOf(other);
		public bool Overlaps(IEnumerable<T> other) => this.wrappedSet.Overlaps(other);
		public bool SetEquals(IEnumerable<T> other) => this.wrappedSet.SetEquals(other);
		public bool Contains(T item) => this.wrappedSet.Contains(item);
		public void CopyTo(T[] array, int arrayIndex) => this.wrappedSet.CopyTo(array, arrayIndex);
		public IEnumerator<T> GetEnumerator() => this.wrappedSet.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

		#region Unsupported (Read-Only)

		public bool Add(T item)
		{
			throw new NotSupportedException("Set is read-only.");
		}

		void ICollection<T>.Add(T item)
		{
			throw new NotSupportedException("Set is read-only.");
		}

		public void UnionWith(IEnumerable<T> other)
		{
			throw new NotSupportedException("Set is read-only.");
		}

		public void IntersectWith(IEnumerable<T> other)
		{
			throw new NotSupportedException("Set is read-only.");
		}

		public void ExceptWith(IEnumerable<T> other)
		{
			throw new NotSupportedException("Set is read-only.");
		}

		public void SymmetricExceptWith(IEnumerable<T> other)
		{
			throw new NotSupportedException("Set is read-only.");
		}

		public void Clear()
		{
			throw new NotSupportedException("Set is read-only.");
		}

		public bool Remove(T item)
		{
			throw new NotSupportedException("Set is read-only.");
		}

		#endregion
	}
}

