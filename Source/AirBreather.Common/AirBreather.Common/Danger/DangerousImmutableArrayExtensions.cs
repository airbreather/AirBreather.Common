using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace AirBreather.Danger
{
    public static class DangerousImmutableArrayExtensions
    {
        public static ImmutableArray<T> AsImmutableArrayDangerous<T>(this T[] array)
        {
            array.ValidateNotNull(nameof(array));

            // on this day, I go to Sovngarde.
            return Unsafe.As<T[], ImmutableArray<T>>(ref array);
        }

        public static T[] AsRegularArrayDangerous<T>(this ImmutableArray<T> array)
        {
            array.ValidateNotDefault(nameof(array));

            // on this day, I go to Sovngarde.
            return Unsafe.As<ImmutableArray<T>, T[]>(ref array);
        }
    }
}
