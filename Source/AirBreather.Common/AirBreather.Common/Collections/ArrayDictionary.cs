using System.Collections.Generic;

using AirBreather.Common.Utilities;

namespace AirBreather.Common.Collections
{
    public sealed class ArrayDictionary<T> : IReadOnlyDictionary<int, T>
    {
        private readonly T[] array;

        public ArrayDictionary(T[] array)
        {
            this.array = array.ValidateNotNull(nameof(array));
        }

        public T this[int key] => this.array[key.ValidateInRange(nameof(key), 0, this.array.Length)];

        public int Count => this.array.Length;

        public IEnumerable<int> Keys => new KeysCollection(this.array.Length);

        public IEnumerable<T> Values => new ValuesCollection(this.array);

        public bool ContainsKey(int key) => key.IsInRange(0, this.array.Length);

        public IEnumerator<KeyValuePair<int, T>> GetEnumerator() => new Enumerator(this.array);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

        public bool TryGetValue(int key, out T value)
        {
            bool result = key.IsInRange(0, this.array.Length);
            value = result ? this.array[key] : default(T);
            return result;
        }

        private sealed class ValuesCollection : IReadOnlyList<T>
        {
            private readonly T[] array;

            internal ValuesCollection(T[] array)
            {
                this.array = array;
            }

            public int Count => this.array.Length;

            public T this[int index] => this.array[index.ValidateInRange(nameof(index), 0, this.array.Length)];

            public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)this.array).GetEnumerator();

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ((System.Collections.IEnumerable)this.array).GetEnumerator();
        }

        private sealed class KeysCollection : IReadOnlyList<int>
        {
            private readonly int max;

            internal KeysCollection(int max)
            {
                this.max = max;
            }

            public int Count => this.max;

            public int this[int index] => index.ValidateInRange(nameof(index), 0, this.max);

            public IEnumerator<int> GetEnumerator() => new Enumerator(this.max);

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

            private sealed class Enumerator : IEnumerator<int>
            {
                private readonly int max;

                private int idx;

                internal Enumerator(int max)
                {
                    this.max = max;
                }

                public int Current => this.idx;

                object System.Collections.IEnumerator.Current => this.Current;

                public void Dispose()
                {
                }

                public bool MoveNext() => this.idx < this.max &&
                                          ++this.idx < this.max;

                public void Reset() => this.idx = -1;
            }
        }

        private sealed class Enumerator : IEnumerator<KeyValuePair<int, T>>
        {
            private readonly T[] array;

            private int idx;

            internal Enumerator(T[] array)
            {
                this.array = array;
            }

            public KeyValuePair<int, T> Current => KeyValuePair.Create(this.idx, this.array[this.idx]);

            object System.Collections.IEnumerator.Current => this.Current;

            public void Dispose()
            {
            }

            public bool MoveNext() => this.idx < this.array.Length &&
                                      ++this.idx < this.array.Length;

            public void Reset() => this.idx = -1;
        }
    }
}
