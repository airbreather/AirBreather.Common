using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AirBreather.Collections
{
    /// <summary>
    /// A slimmer analog of <see cref="List{T}"/> that tries to optimize for minimum size overhead
    /// beyond <c>T[]</c>, while still providing the same core functionality.  This can have a speed
    /// cost (compared to equivalent <see cref="List{T}"/> usage) when used naively, and it can be
    /// more dangerous because it intentionally skips "modified while iterating" checks.
    /// </summary>
    /// <typeparam name="T">
    /// The type of element stored in the list.
    /// </typeparam>
    /// <remarks>
    /// This is almost entirely untested at this point.
    /// </remarks>
    public sealed class SlimList<T> : IList<T>, IReadOnlyList<T>
    {
        private T[] arr = Array.Empty<T>();

        public SlimList() { }

        public SlimList(int capacity) => this.arr = new T[capacity];

        public SlimList(IEnumerable<T> items) => this.AddRange(items.ValidateNotNull(nameof(items)));

        public T this[int index]
        {
            get => this.arr[index];
            set => this.arr[index] = value;
        }

        public T[] DangerousCurrentArray => this.arr;

        public int Count { get; private set; }

        public int Capacity
        {
            get => this.arr.Length;
            set => Array.Resize(ref this.arr, value);
        }

        bool ICollection<T>.IsReadOnly => false;

        public void Add(T item)
        {
            if (this.Count++ == this.arr.Length)
            {
                Array.Resize(ref this.arr, this.Count);
            }

            this.arr[this.Count - 1] = item;
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (!items.TryGetCount(out var cnt))
            {
                // it's too inefficient to just add one-by-one.  just buffer it.
                List<T> itemsList = items.ToList();
                items = itemsList;
                cnt = itemsList.Count;
            }

            int newCount = this.Count + cnt;
            if (newCount > this.arr.Length)
            {
                Array.Resize(ref this.arr, newCount);
            }

            items.CopyTo(this.arr, this.Count);
            this.Count = newCount;
        }

        public void Clear()
        {
            this.arr = Array.Empty<T>();
            this.Count = 0;
        }

        public bool Contains(T item) => 0 <= this.IndexOf(item);

        public void CopyTo(T[] array, int arrayIndex) => Array.Copy(this.arr, 0, array, arrayIndex, this.Count);

        public T Find(Predicate<T> pred)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (pred(this.arr[i]))
                {
                    return this.arr[i];
                }
            }

            return default(T);
        }

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public int IndexOf(T item) => Array.IndexOf(this.arr, item, 0, this.Count);

        public void Insert(int index, T item)
        {
            if (index == this.Count)
            {
                this.Add(item);
                return;
            }

            if (this.Count++ == this.arr.Length)
            {
                T[] newArr = new T[this.Count];
                Array.Copy(this.arr, 0, newArr, 0, index);
                Array.Copy(this.arr, index, newArr, index + 1, this.Count - index - 1);
            }
            else
            {
                for (int i = this.Count - 2; i >= index; i--)
                {
                    this.arr[i + 1] = this.arr[i];
                }
            }

            this.arr[index] = item;
        }

        public bool Remove(T item)
        {
            int index = this.IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            this.RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (index < --this.Count)
            {
                Array.Copy(this.arr, index + 1, this.arr, index, this.Count - index);
            }

            this.arr[this.Count] = default(T);
        }

        public void Sort(IComparer<T> comparer) => Array.Sort(this.arr, 0, this.Count, comparer);

        public void Trim() => Array.Resize(ref this.arr, this.Count);

        public struct Enumerator : IEnumerator<T>
        {
            private SlimList<T> lst;

            private int pos;

            internal Enumerator(SlimList<T> lst)
            {
                this.lst = lst;
                this.pos = -1;
            }

            public bool MoveNext() => this.pos < this.lst.Count &&
                                      ++this.pos < this.lst.Count;

            public T Current => this.lst[this.pos];

            object IEnumerator.Current => this.Current;

            public void Dispose() { }

            public void Reset() => this.pos = -1;
        }
    }
}
