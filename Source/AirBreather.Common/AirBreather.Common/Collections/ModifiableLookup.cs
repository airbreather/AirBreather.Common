using System.Collections.Generic;
using System.Linq;

namespace AirBreather
{
    public sealed class ModifiableLookup<TKey, TValue> : IModifiableLookup<TKey, TValue>
    {
        private readonly Dictionary<TKey, ModifiableGrouping> backingStore;

        public ModifiableLookup()
        {
            this.backingStore = new Dictionary<TKey, ModifiableGrouping>();
        }

        public ModifiableLookup(IEqualityComparer<TKey> keyComparer)
        {
            this.backingStore = new Dictionary<TKey, ModifiableGrouping>(keyComparer);
        }

        public ModifiableLookup(int capacity)
        {
            this.backingStore = new Dictionary<TKey, ModifiableGrouping>(capacity);
        }

        public ModifiableLookup(int capacity, IEqualityComparer<TKey> keyComparer)
        {
            this.backingStore = new Dictionary<TKey, ModifiableGrouping>(capacity, keyComparer);
        }

        public void Add(TKey key, TValue value)
        {
            ModifiableGrouping grouping;
            if (this.backingStore.TryGetValue(key, out grouping))
            {
                grouping.Add(value);
            }
            else
            {
                this.backingStore.Add(key, new ModifiableGrouping(key, value));
            }
        }

        public void AddRange(TKey key, IEnumerable<TValue> values)
        {
            ModifiableGrouping grouping;
            if (this.backingStore.TryGetValue(key, out grouping))
            {
                grouping.AddRange(values);
            }
            else
            {
                grouping = new ModifiableGrouping(key, values);
                if (grouping.Count > 0)
                {
                    this.backingStore.Add(key, grouping);
                }
            }
        }

        public void AddRange(IEnumerable<IGrouping<TKey, TValue>> groups)
        {
            foreach (IGrouping<TKey, TValue> grp in groups)
            {
                this.AddRange(grp.Key, grp);
            }
        }

        public bool Remove(TKey key) => this.backingStore.Remove(key);

        public bool Remove(TKey key, TValue value)
        {
            ModifiableGrouping grouping;
            if (!this.backingStore.TryGetValue(key, out grouping))
            {
                return false;
            }

            if (!grouping.Remove(value))
            {
                return false;
            }

            if (grouping.Count == 0)
            {
                this.backingStore.Remove(key);
            }

            return true;
        }

        public void Clear() => this.backingStore.Clear();
        public bool Contains(TKey key) => this.backingStore.ContainsKey(key);
        public int Count => this.backingStore.Count;

        public IEnumerable<TValue> this[TKey index]
        {
            get
            {
                ModifiableGrouping grouping;
                return this.backingStore.TryGetValue(index, out grouping)
                    ? grouping
                    : Enumerable.Empty<TValue>();
            }
        }

        public IEnumerator<IGrouping<TKey, TValue>> GetEnumerator() => this.backingStore.Values.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

        // Don't bother trying to implement "you can get a modifiable grouping from the lookup
        // and affect the parent lookup by modifying the child grouping".  It's too complicated.
        private sealed class ModifiableGrouping : IGrouping<TKey, TValue>, IReadOnlyList<TValue>
        {
            private readonly List<TValue> backingStore;

            internal ModifiableGrouping(TKey key, TValue value)
            {
                this.Key = key;
                this.backingStore = new List<TValue> { value };
            }

            internal ModifiableGrouping(TKey key, IEnumerable<TValue> values)
            {
                this.Key = key;
                this.backingStore = values.ToList();
            }

            public TKey Key { get; }
            public int Count => this.backingStore.Count;
            public TValue this[int index] => this.backingStore[index];

            internal void Add(TValue value) => this.backingStore.Add(value);
            internal void AddRange(IEnumerable<TValue> values) => this.backingStore.AddRange(values);
            internal bool Remove(TValue value) => this.backingStore.Remove(value);

            public IEnumerator<TValue> GetEnumerator() => this.backingStore.GetEnumerator();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator(); 
        }
    }
}

