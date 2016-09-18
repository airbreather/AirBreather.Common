using System.Collections.Generic;
using System.Linq;

namespace AirBreather.Collections
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

        public IEnumerable<int> Keys => Enumerable.Range(0, this.array.Length);

        public IEnumerable<T> Values => this.array.Skip(0);

        public bool ContainsKey(int key) => key.IsInRange(0, this.array.Length);

        public IEnumerator<KeyValuePair<int, T>> GetEnumerator() => this.array.Select(KeyValuePair.CreateInverted).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

        public bool TryGetValue(int key, out T value)
        {
            bool result = key.IsInRange(0, this.array.Length);
            value = result ? this.array[key] : default(T);
            return result;
        }
    }
}
