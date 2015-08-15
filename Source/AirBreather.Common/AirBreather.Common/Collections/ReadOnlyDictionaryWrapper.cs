using System;
using System.Collections.Generic;
using System.Linq;

using AirBreather.Common.Utilities;

namespace AirBreather.Common.Collections
{
    public sealed class ReadOnlyDictionaryWrapper<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IDictionary<TKey, TValue>
    {
        private readonly IReadOnlyDictionary<TKey, TValue> wrappedDictionary;

        public ReadOnlyDictionaryWrapper(IReadOnlyDictionary<TKey, TValue> dictionary)
        {
            this.wrappedDictionary = dictionary.ValidateNotNull(nameof(dictionary));
        }

        public bool IsReadOnly => true;
        public int Count => this.wrappedDictionary.Count;

        public TValue this[TKey key] => this.wrappedDictionary[key];

        public IEnumerable<TKey> Keys => this.wrappedDictionary.Keys;
        public IEnumerable<TValue> Values => this.wrappedDictionary.Values;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                IEnumerable<TKey> result = this.wrappedDictionary.Keys;
                return result == null
                    ? null
                    : result as ICollection<TKey> ?? result.ToArray();
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                IEnumerable<TValue> result = this.wrappedDictionary.Values;
                return result == null
                    ? null
                    : result as ICollection<TValue> ?? result.ToArray();
            }
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get { return this[key]; }
            set { throw new NotSupportedException(); }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => this.wrappedDictionary.Contains(item);
        public bool ContainsKey(TKey key) => this.wrappedDictionary.ContainsKey(key);
        public bool TryGetValue(TKey key, out TValue value) => this.wrappedDictionary.TryGetValue(key, out value);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => this.AsReadOnlyCollection().CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => this.wrappedDictionary.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.wrappedDictionary.GetEnumerator();

        public void Add(TKey key, TValue value)
        {
            throw new NotSupportedException();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }
        public bool Remove(TKey key)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }
    }
}
