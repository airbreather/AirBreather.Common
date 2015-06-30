using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AirBreather.Common.Collections
{
    public sealed class ReadOnlyCollectionWrapper<T> : IReadOnlyCollection<T>, ICollection<T>
    {
        private readonly IReadOnlyCollection<T> wrappedCollection;

        public ReadOnlyCollectionWrapper(IReadOnlyCollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            this.wrappedCollection = collection;
        }

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            return this.wrappedCollection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
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

            if (array.Length - arrayIndex < this.Count)
            {
                throw new ArgumentException("Not enough room", nameof(array));
            }

            foreach (T item in this.wrappedCollection)
            {
                array[arrayIndex++] = item;
            }
        }

        public int Count
        {
            get { return this.wrappedCollection.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.wrappedCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.wrappedCollection.GetEnumerator();
        }
    }
}
