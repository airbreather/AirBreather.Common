using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AirBreather.Common.Collections
{
    public sealed class ReadOnlyListWrapper<T> : IReadOnlyList<T>, IList<T>
    {
        private readonly IReadOnlyList<T> wrappedList;

        public ReadOnlyListWrapper(IReadOnlyList<T> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            this.wrappedList = list;
        }

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

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public T this[int index]
        {
            get
            {
                return this.wrappedList[index];
            }
            set
            {
                throw new NotSupportedException();
            }
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
            return this.wrappedList.Contains(item);
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

            for (int i = 0; i < this.Count; i++)
            {
                array[i + arrayIndex] = this.wrappedList[i];
            }
        }

        public int Count
        {
            get { return this.wrappedList.Count; }
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
            return this.wrappedList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.wrappedList.GetEnumerator();
        }
    }
}
