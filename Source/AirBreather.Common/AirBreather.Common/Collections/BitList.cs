using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AirBreather.Common.Collections
{
    public sealed class BitList : IList<bool>, IReadOnlyList<bool>
    {
        private readonly List<int> values;

        private int version;

        public BitList()
        {
            this.values = new List<int>();
        }

        public BitList(BitArray bitArray)
        {
            if (bitArray == null)
            {
                throw new ArgumentNullException("bitArray");
            }

            int length = bitArray.Length;
            int valCount = length / 32;
            if (length % 32 > 0)
            {
                valCount++;
            }

            int[] array = new int[valCount];
            bitArray.CopyTo(array, 0);
            this.values = new List<int>(array);
            this.Count = length;
        }

        private BitList(IEnumerable<int> values)
        {
            this.values = values.ToList();
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

        public int Count { get; private set; }

        public bool IsReadOnly { get { return false; } }

        public int IndexOf(bool item)
        {
            // TODO: optimize this.
            // we could compare 32 bits at a time,
            // only going bit-by-bit for the last integer
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i] == item)
                {
                    return i;
                }
            }

            return -1;
        }

        public void Insert(int index, bool item)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", index, "Must be non-negative.");
            }

            if (this.Count < index)
            {
                throw new ArgumentOutOfRangeException("index", index, "Too big.");
            }

            if (this.Count == index)
            {
                this.AddCore(item);
            }
            else
            {
                // TODO: optimize this
                BitList copied = new BitList(this.values) { Count = this.Count };
                this.values.Clear();
                this.Count = 0;
                for (int i = 0; i <= copied.Count; i++)
                {
                    bool next;

                    if (i == index)
                    {
                        next = item;
                    }
                    else if (i > index)
                    {
                        next = copied[i - 1];
                    }
                    else
                    {
                        next = copied[i];
                    }

                    this.AddCore(next);
                }
            }

            this.Modified();
        }

        public void RemoveAt(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", index, "Must be non-negative.");
            }

            if (this.Count <= index)
            {
                throw new ArgumentOutOfRangeException("index", index, "Too big.");
            }

            // TODO: optimize this
            BitList copied = new BitList(this.values) { Count = this.Count };
            this.values.Clear();
            this.Count = 0;
            for (int i = 0; i < copied.Count; i++)
            {
                if (i == index)
                {
                    continue;
                }

                this.AddCore(copied[i]);
            }

            this.Modified();
        }

        public bool this[int index]
        {
            get
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException("index", index, "Must be non-negative.");
                }

                if (this.Count <= index)
                {
                    throw new ArgumentOutOfRangeException("index", index, "Too big.");
                }

                return (this.values[index / 32] & (1u << (index % 32))) > 0;
            }

            set
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException("index", index, "Must be non-negative.");
                }

                if (this.Count <= index)
                {
                    throw new ArgumentOutOfRangeException("index", index, "Too big.");
                }

                int mask = unchecked((int)1u << (index % 32));
                if (value)
                {
                    this.values[index / 32] |= mask;
                }
                else
                {
                    this.values[index / 32] &= ~mask;
                }

                this.Modified();
            }
        }

        public void Add(bool value)
        {
            this.AddCore(value);
            this.Modified();
        }

        public void Clear()
        {
            this.values.Clear();
            this.Count = 0;
            this.Modified();
        }

        public bool Contains(bool item)
        {
            // TODO: this can be optimized slightly more than IndexOf can be, because in addition to
            // skipping 32-bit integers that aren't exactly the mask, it can also just create a new
            // mask for the last one based on Count % 32 and just return whether or not the final
            // integer matches the mask.
            return this.IndexOf(item) >= 0;
        }

        public void CopyTo(bool[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException("arrayIndex", arrayIndex, "Must be non-negative.");
            }

            if (array.Length <= arrayIndex)
            {
                throw new ArgumentOutOfRangeException("arrayIndex", arrayIndex, "Must be less than the length of the array.");
            }

            if (array.Length - arrayIndex < this.Count)
            {
                throw new ArgumentException("Not enough room", "array");
            }

            for (int i = 0; i < this.Count; i++)
            {
                array[arrayIndex + i] = this[i];
            }
        }

        public BitArray ToBitArray()
        {
            return new BitArray(this.values.ToArray())
            {
                Length = this.Count
            };
        }

        public void TrimExcess()
        {
            this.values.TrimExcess();
        }

        public IEnumerator<bool> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private void AddCore(bool value)
        {
            int offset = this.Count % 32;
            if (offset == 0)
            {
                this.values.Add(0);
            }

            if (value)
            {
                this.values[this.values.Count - 1] |= (1 << offset);
            }

            this.Count++;
        }

        private void Modified()
        {
            unchecked
            {
                this.version++;
            }
        }

        private sealed class Enumerator : IEnumerator<bool>
        {
            private readonly BitList lst;

            private readonly int version;

            private int currIndex;

            private bool curr;

            internal Enumerator(BitList lst)
            {
                this.lst = lst;
                this.version = lst.version;
                this.currIndex = 0;
                this.curr = false;
            }

            public bool Current
            {
                get { return this.curr; }
            }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

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

                this.curr = this.lst[this.currIndex++];
                return true;
            }

            public void Reset()
            {
                if (this.version != this.lst.version)
                {
                    throw new InvalidOperationException("Collection was modified during enumeration.");
                }

                this.currIndex = 0;
            }

            void IDisposable.Dispose()
            {
            }
        }
    }
}
