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

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }
    }
}
