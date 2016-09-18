using System;
using System.Collections.Generic;
using System.Linq;

namespace AirBreather.Collections
{
    public sealed class ReadOnlyListWrapper<T> : IReadOnlyList<T>, IList<T>
    {
        private readonly IReadOnlyList<T> wrappedList;

        public ReadOnlyListWrapper(IReadOnlyList<T> list)
        {
            this.wrappedList = list.ValidateNotNull(nameof(list));
        }

        public bool IsReadOnly => true;
        public int Count => this.wrappedList.Count;

        public int IndexOf(T item)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (Equals(this.wrappedList[i], item))
                {
                    return i;
                }
            }

            return -1;
        }

        public T this[int index] => this.wrappedList[index];

        T IList<T>.this[int index]
        {
            get { return this[index]; }
            set { throw new NotSupportedException(); }
        }

        public bool Contains(T item) => this.wrappedList.Contains(item);
        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => this.AsReadOnlyCollection().CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => this.wrappedList.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.wrappedList.GetEnumerator();

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

        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
