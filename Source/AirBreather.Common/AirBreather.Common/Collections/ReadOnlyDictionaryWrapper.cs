using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirBreather.Common.Collections
{
    public sealed class ReadOnlyDictionaryWrapper<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IDictionary<TKey, TValue>
    {
        private readonly IReadOnlyDictionary<TKey, TValue> wrappedDictionary;

        public ReadOnlyDictionaryWrapper(IReadOnlyDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }

            this.wrappedDictionary = dictionary;
        }

        public void Add(TKey key, TValue value)
        {
            throw new NotSupportedException();
        }

        public bool ContainsKey(TKey key)
        {
            return this.wrappedDictionary.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get
            {
                IEnumerable<TKey> result = this.wrappedDictionary.Keys;
                return result == null
                    ? null
                    : result as ICollection<TKey> ?? result.ToArray();
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get { return this.wrappedDictionary.Keys; }
        }

        public bool Remove(TKey key)
        {
            throw new NotSupportedException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.wrappedDictionary.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values
        {
            get
            {
                IEnumerable<TValue> result = this.wrappedDictionary.Values;
                return result == null
                    ? null
                    : result as ICollection<TValue> ?? result.ToArray();
            }
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get { return this.wrappedDictionary.Values; }
        }

        public TValue this[TKey key]
        {
            get
            {
                return this.wrappedDictionary[key];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.wrappedDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
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

            foreach (KeyValuePair<TKey, TValue> kvp in this.wrappedDictionary)
            {
                array[arrayIndex++] = kvp;
            }
        }

        public int Count
        {
            get { return this.wrappedDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.wrappedDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.wrappedDictionary.GetEnumerator();
        }
    }
}
