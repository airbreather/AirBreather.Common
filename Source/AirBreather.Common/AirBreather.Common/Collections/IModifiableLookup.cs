using System.Collections.Generic;
using System.Linq;

namespace AirBreather.Common
{
    public interface IModifiableLookup<TKey, TValue> : ILookup<TKey, TValue>
    {
        void Add(TKey key, TValue value);
        void AddRange(TKey key, IEnumerable<TValue> values);
        void AddRange(IEnumerable<IGrouping<TKey, TValue>> groups);

        bool Remove(TKey key);
        bool Remove(TKey key, TValue value);

        void Clear();
    }
}
    