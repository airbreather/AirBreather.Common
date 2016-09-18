using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AirBreather.Collections
{
    public sealed class ObjectReferenceEqualityComparer<T> : EqualityComparer<T> where T : class
    {
        public override bool Equals(T x, T y) => ReferenceEquals(x, y);

        public override int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
