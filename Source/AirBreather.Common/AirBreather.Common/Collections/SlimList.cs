using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AirBreather.Collections
{
    public sealed class SlimList<T> : IList<T>, IReadOnlyList<T>
    {
        private T[] arr = Array.Empty<T>();

        private int pos;

        public SlimList()
        {
        }

        public SlimList(int capacity)
        {
            this.arr = new T[capacity];
        }

        public SlimList(IEnumerable<T> items)
        {
            this.arr = items.ToArray();
            this.pos = this.arr.Length;
        }

        public T this[int index]
        {
            get { return this.arr[index]; }
            set { this.arr[index] = value; }
        }

        public int Count => this.pos;

        public int Capacity
        {
            get { return this.arr.Length; }
            set { Array.Resize(ref this.arr, value); }
        }

        bool ICollection<T>.IsReadOnly => false;

        public void Add(T item)
        {
            if (this.pos++ == this.arr.Length)
            {
                Array.Resize(ref this.arr, this.pos);
            }

            this.arr[this.pos - 1] = item;
        }

        public void AddRange(IEnumerable<T> items)
        {
            int cnt;
            if (!items.TryGetCount(out cnt))
            {
                // it's too inefficient to just add one-by-one.  just buffer it.
                List<T> itemsList = items.ToList();
                items = itemsList;
                cnt = itemsList.Count;
            }

            if (this.pos + cnt > this.arr.Length)
            {
                Array.Resize(ref this.arr, this.pos + cnt);
            }

            foreach (var item in items)
            {
                this.arr[this.pos++] = item;
            }
        }

        public void Clear()
        {
            this.arr = Array.Empty<T>();
            this.pos = 0;
        }

        public bool Contains(T item) => 0 <= this.IndexOf(item);

        public void CopyTo(T[] array, int arrayIndex) => Array.Copy(this.arr, 0, array, arrayIndex, this.pos);

        public T Find(Predicate<T> pred)
        {
            for (int i = 0; i < this.pos; i++)
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

        public int IndexOf(T item) => Array.IndexOf(this.arr, item, 0, this.pos);

        public void Insert(int index, T item)
        {
            if (index == this.pos)
            {
                this.Add(item);
                return;
            }

            if (this.pos++ == this.arr.Length)
            {
                T[] newArr = new T[this.pos];
                Array.Copy(this.arr, 0, newArr, 0, index);
                Array.Copy(this.arr, index, newArr, index + 1, this.pos - index - 1);
            }
            else
            {
                for (int i = this.pos - 2; i >= index; i--)
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
            if (index < --this.pos)
            {
                Array.Copy(this.arr, index + 1, this.arr, index, this.pos - index);
            }

            this.arr[this.pos] = default(T);
        }

        public void Sort(IComparer<T> comparer) => Array.Sort(this.arr, 0, this.pos, comparer);

        public void Trim() => Array.Resize(ref this.arr, this.pos);

        public struct Enumerator : IEnumerator<T>
        {
            private SlimList<T> lst;

            private int pos;

            internal Enumerator(SlimList<T> lst)
            {
                this.lst = lst;
                this.pos = 0;
            }

            public bool MoveNext() => this.pos < this.lst.pos &&
                                      ++this.pos < this.lst.pos;

            public T Current => this.lst[this.pos];

            object IEnumerator.Current => this.Current;

            public void Dispose() { }

            public void Reset() => this.pos = 0;
        }
    }
}
