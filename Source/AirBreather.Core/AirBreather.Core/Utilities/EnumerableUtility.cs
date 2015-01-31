using System;
using System.Collections.Generic;
using System.Linq;

namespace AirBreather.Core.Utilities
{
    public static class EnumerableUtility
    {
        // lets me write this:
        //     someEnumerable.ExceptWhere(someSet.Contains)
        // instead of this:
        //     someEnumerable.Where(x => !someSet.Contains(x))
        // and this also slightly improves fluent readability
        public static IEnumerable<T> ExceptWhere<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            // LINQ's .Where() has lots of optimizations,
            // so use that instead of re-implementing it.
            return enumerable.Where(x => !predicate(x));
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.ToHashSet(null);
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable, IEqualityComparer<T> equalityComparer)
        {
            return new HashSet<T>(enumerable, equalityComparer);
        }
    }
}
