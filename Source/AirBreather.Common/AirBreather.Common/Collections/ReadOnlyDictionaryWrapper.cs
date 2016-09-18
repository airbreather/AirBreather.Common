using System;
using System.Collections.Generic;
using System.Linq;

namespace AirBreather.Collections
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
                    : result as ICollection<TKey> ?? result.ToList();
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                IEnumerable<TValue> result = this.wrappedDictionary.Values;
                return result == null
                    ? null
                    : result as ICollection<TValue> ?? result.ToList();
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

        #region Unsupported (Read-Only)

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
