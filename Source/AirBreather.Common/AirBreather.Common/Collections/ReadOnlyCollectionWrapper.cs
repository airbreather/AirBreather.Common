using System;
using System.Collections.Generic;
using System.Linq;

using AirBreather.Common.Utilities;

namespace AirBreather.Common.Collections
{
    public sealed class ReadOnlyCollectionWrapper<T> : IReadOnlyCollection<T>, ICollection<T>
    {
        private readonly IReadOnlyCollection<T> wrappedCollection;

        public ReadOnlyCollectionWrapper(IReadOnlyCollection<T> collection)
        {
            this.wrappedCollection = collection.ValidateNotNull(nameof(collection));
        }

        public bool IsReadOnly => true;
        public int Count => this.wrappedCollection.Count;

        public bool Contains(T item) => this.wrappedCollection.Contains(item);
        public IEnumerator<T> GetEnumerator() => this.wrappedCollection.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.wrappedCollection.GetEnumerator();
        public void CopyTo(T[] array, int arrayIndex) => this.AsReadOnlyCollection().CopyTo(array, arrayIndex);

        #region Unsupported (Read-Only)

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
