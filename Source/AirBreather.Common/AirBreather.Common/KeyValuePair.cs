using System.Collections.Generic;

namespace AirBreather
{
    public static class KeyValuePair
    {
        public static KeyValuePair<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value) => new KeyValuePair<TKey, TValue>(key, value);

        public static KeyValuePair<TKey, TValue> CreateInverted<TKey, TValue>(TValue value, TKey key) => new KeyValuePair<TKey, TValue>(key, value);

        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
}
