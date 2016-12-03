using System;
using System.Collections;
using System.Collections.Generic;

namespace AirBreather.Collections
{
    public sealed class BitList : IList<bool>, IReadOnlyList<bool>
    {
        private static readonly BitArray Empty = new BitArray(0);

        private BitArray values;

        private int version;

        public BitList() => this.values = Empty;

        public BitList(int capacity) => this.values = capacity.ValidateNotLessThan(nameof(capacity), 0) == 0
            ? Empty
            : new BitArray(capacity);

        public BitList(BitArray bitArray)
        {
            this.values = bitArray.ValidateNotNull(nameof(bitArray)).Length == 0
                ? Empty
                : new BitArray(bitArray);

            this.Count = this.values.Length;
        }

        public BitList(IEnumerable<bool> values)
        {
            switch (values.ValidateNotNull(nameof(values)))
            {
                case bool[] array:
                    this.values = new BitArray(array);
                    this.Count = array.Length;
                    break;

                case BitList lst:
                    this.values = new BitArray(lst.values);
                    this.Count = lst.Count;
                    break;

                default:
                    this.values = values.TryGetCount(out var trueInitialCapacity) && trueInitialCapacity != 0
                        ? new BitArray(trueInitialCapacity)
                        : Empty;

                    // Don't bother with AddRange -- it does extra stuff we've already done.
                    foreach (bool value in values)
                    {
                        this.AddCore(value);
                    }

                    break;
            }
        }

        public int Count { get; private set; }

        bool ICollection<bool>.IsReadOnly => false;

        public int Capacity
        {
            get => this.values.Length;

            set
            {
                if (value < this.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Too many elements.");
                }

                if (value == this.values.Length)
                {
                    return;
                }

                if (this.values == Empty)
                {
                    this.values = new BitArray(value);
                }
                else
                {
                    this.values.Length = value;
                }
            }
        }

        public bool this[int index]
        {
            get => this.values[index.ValidateInRange(nameof(index), 0, this.Count)];

            set
            {
                this.values[index.ValidateInRange(nameof(index), 0, this.Count)] = value;
                this.Modified();
            }
        }

        public void Add(bool value)
        {
            this.AddCore(value);
            this.Modified();
        }

        private void AddCore(bool value)
        {
            this.EnsureCapacity(this.Count + 1);
            this.values[this.Count++] = value;
        }

        public void AddRange(IEnumerable<bool> values) => this.InsertRange(this.Count, values);

        public void Clear()
        {
            // unlike List<T>, we have absolutely no reason to clear the array itself.
            this.Count = 0;
            this.Modified();
        }

        public bool Contains(bool value)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this.values[i] == value)
                {
                    return true;
                }
            }

            return false;
        }

        void ICollection<bool>.CopyTo(bool[] array, int arrayIndex) => this.AsReadOnlyCollection().CopyTo(array, arrayIndex);

        private void EnsureCapacity(int neededSize)
        {
            if (this.values.Length < neededSize)
            {
                this.Capacity = (int)Math.Min(Int32.MaxValue,
                                              Math.Max(neededSize,
                                                       unchecked((uint)(this.values.Length == 0
                                                                            ? 128
                                                                            : this.values.Length * 2))));
            }
        }

        public void ForEach(Action<bool> action)
        {
            action.ValidateNotNull(nameof(action));

            foreach (bool value in this)
            {
                action(value);
            }
        }

        public int IndexOf(bool item)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this.values[i] == item)
                {
                    return i;
                }
            }

            return -1;
        }

        public void Insert(int index, bool value)
        {
            this.InsertCore(index.ValidateInRange(nameof(index), 0, this.Count + 1), value);
            this.Modified();
        }

        private void InsertCore(int index, bool value)
        {
            this.EnsureCapacity(this.Count + 1);
            for (int i = this.Count - 1; i >= index; i--)
            {
                this.values[i + 1] = this.values[i];
            }

            this.values[index] = value;
            this.Count++;
        }

        public void InsertRange(int index, IEnumerable<bool> values)
        {
            index.ValidateInRange(nameof(index), 0, this.Count + 1);
            if (values.ValidateNotNull(nameof(values)).TryGetCount(out var insertCount))
            {
                int trueInsertCount = insertCount;

                this.EnsureCapacity(this.Count + trueInsertCount);

                // shift the values after the index over by the width of what we're inserting.
                for (int i = this.Count - 1; i >= index; --i)
                {
                    this.values[i + trueInsertCount] = this.values[i];
                }

                if (this == values)
                {
                    for (int i = index - 1; i >= 0; --i)
                    {
                        this.values[i + index] = this.values[i];
                    }

                    for (int i = this.Count - 1; i >= index; --i)
                    {
                        this.values[i + index + index] = this.values[i + index + trueInsertCount];
                    }

                    return;
                }

                foreach (bool value in values)
                {
                    this.values[index++] = value;
                }

                this.Count += trueInsertCount;
            }
            else
            {
                // ugh.
                foreach (bool value in values)
                {
                    this.InsertCore(index++, value);
                }
            }

            this.Modified();
        }

        public bool Remove(bool item)
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
            index.ValidateInRange(nameof(index), 0, this.Count);
            for (int i = index; i < this.Count - 1; i++)
            {
                this.values[i] = this.values[i + 1];
            }

            this.Count--;
            this.Modified();
        }

        public void RemoveRange(int index, int count)
        {
            index.ValidateNotLessThan(nameof(index), 0);
            count.ValidateNotLessThan(nameof(count), 0);

            if (count == 0)
            {
                return;
            }

            if (this.Count - index < count)
            {
                throw new ArgumentException("Not enough values to remove", nameof(count));
            }

            this.Count -= count;
            for (int i = index; i < this.Count; i++)
            {
                this.values[i] = this.values[i + count];
            }

            this.Modified();
        }

        public void Reverse()
        {
            int i = 0;
            int j = this.Count - 1;
            while (i < j)
            {
                bool temp = this.values[i];
                this.values[i++] = this.values[j];
                this.values[j--] = temp;
            }

            this.Modified();
        }

        public BitArray ToBitArray()
        {
            BitArray result = new BitArray(this.Count);
            for (int i = 0; i < this.Count; i++)
            {
                result[i] = this[i];
            }

            return result;
        }

        public void TrimExcess() => this.Capacity = this.Count;

        public IEnumerator<bool> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        private void Modified() => this.version++;

        private sealed class Enumerator : IEnumerator<bool>
        {
            private readonly BitList lst;

            private readonly int version;

            private int currIndex;

            internal Enumerator(BitList lst)
            {
                this.lst = lst;
                this.version = lst.version;
                this.currIndex = 0;
            }

            public bool Current { get; private set; }

            object IEnumerator.Current => Boxes.Boolean(this.Current);

            public bool MoveNext()
            {
                if (this.version != this.lst.version)
                {
                    throw new InvalidOperationException("Collection was modified during enumeration.");
                }

                if (this.lst.Count <= this.currIndex)
                {
                    return false;
                }

                this.Current = this.lst[this.currIndex++];
                return true;
            }

            public void Reset() => this.currIndex = this.version == this.lst.version
                ? 0
                : throw new InvalidOperationException("Collection was modified during enumeration.");

            void IDisposable.Dispose() { }
        }
    }
}
