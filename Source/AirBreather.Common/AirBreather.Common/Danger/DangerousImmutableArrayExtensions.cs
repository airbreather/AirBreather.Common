using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace AirBreather.Danger
{
    public static class DangerousImmutableArrayExtensions
    {
        public static ImmutableArray<T> AsImmutableArrayDangerous<T>(this T[] array) => Unsafe.As<T[], ImmutableArray<T>>(ref array);
        public static T[] AsRegularArrayDangerous<T>(this ImmutableArray<T> array) => Unsafe.As<ImmutableArray<T>, T[]>(ref array);
    }
}
