using System;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;

namespace AirBreather.Danger
{
    public static class MyImmutableArrayExtensions
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

        // these next ones are actually comparatively safe.
        public static ReadOnlySpan<T> Slice<T>(this ImmutableArray<T> array) => SliceCore(array, 0, null);
        public static ReadOnlySpan<T> Slice<T>(this ImmutableArray<T> array, int index, int count) => SliceCore(array, index, count);
        private static ReadOnlySpan<T> SliceCore<T>(ImmutableArray<T> array, int index, int? count)
        {
            array.ValidateNotDefault(nameof(array));
            index.ValidateInRange(nameof(index), 0, array.Length);
            if (array.Length - index < count)
            {
                throw new ArgumentException("Not enough room", nameof(array));
            }

            return new ReadOnlySpan<T>(array.AsRegularArrayDangerous(), start: index, length: count ?? array.Length);
        }

        public static MemoryStream ToReadableStream(this ImmutableArray<byte> array) => ToReadableStreamCore(array, 0, null);
        public static MemoryStream ToReadableStream(this ImmutableArray<byte> array, int index, int count) => ToReadableStreamCore(array, index, count);
        private static MemoryStream ToReadableStreamCore(ImmutableArray<byte> array, int index, int? count)
        {
            array.ValidateNotDefault(nameof(array));
            index.ValidateInRange(nameof(index), 0, array.Length);
            if (array.Length - index < count)
            {
                throw new ArgumentException("Not enough room", nameof(array));
            }

            return new MemoryStream(buffer: array.AsRegularArrayDangerous(), index: index, count: count ?? array.Length, writable: false, publiclyVisible: false);
        }
    }
}
