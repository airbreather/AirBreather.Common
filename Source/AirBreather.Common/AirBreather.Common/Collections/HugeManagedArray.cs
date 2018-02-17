using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

using static System.Runtime.CompilerServices.Unsafe;

namespace AirBreather.Collections
{
    internal static class HugeManagedArray
    {
        internal static HugeManagedArrayBlock[] AllocateUnderlying(int sz, long length) => new HugeManagedArrayBlock[Math.DivRem(sz * length, SizeOf<HugeManagedArrayBlock>(), out long rem) + (rem == 0 ? 0 : 1)];

        // the rest of this "helpers" class is based on:
        // https://github.com/dotnet/corefx/blob/1c9cd8118abfa13165d88a4e660fbdcc6c2ebc4c/src/System.Memory/src/System/SpanHelpers.cs#L125-L163
        // TODO: remove after we can use the following to tell the compiler to do this for us:
        // https://github.com/dotnet/csharplang/blob/master/proposals/blittable.md
        internal static bool ContainsReferences<T>() where T : struct => PerTypeValues<T>.ContainsReferences;

        private const BindingFlags DeclaredInstanceFields = BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        private static bool ContainsReferencesCore(Type type)
        {
            if (type.IsPrimitive)
            {
                return false;
            }

            if (!type.IsValueType)
            {
                return true;
            }

            Type underlyingNullable = Nullable.GetUnderlyingType(type);
            if (underlyingNullable != null)
            {
                type = underlyingNullable;
            }

            if (type.IsEnum)
            {
                return false;
            }

            foreach (FieldInfo field in type.GetFields(DeclaredInstanceFields))
            {
                if (ContainsReferencesCore(field.FieldType))
                {
                    return true;
                }
            }

            return false;
        }

        private static class PerTypeValues<T>
            where T : struct // blittable
        {
            internal static readonly bool ContainsReferences = ContainsReferencesCore(typeof(T));
        }
    }

    // at this block size, with <gcAllowVeryLargeObjects> enabled, a single array can hold up to
    // just barely under 128 TB of data (65532 * 0x7FEFFFFF bytes), provided enough virtual memory.
    [StructLayout(LayoutKind.Sequential, Size = 65532)]
    internal struct HugeManagedArrayBlock { }

    public sealed class HugeManagedArray<T> : IEnumerable<T>
        where T : struct // blittable
    {
        private readonly HugeManagedArrayBlock[] blocks;

        public HugeManagedArray(long length)
        {
            this.ValidateBlittable();
            this.Length = length.ValidateNotLessThan(nameof(length), 0);
            this.blocks = length == 0
                ? Array.Empty<HugeManagedArrayBlock>()
                : HugeManagedArray.AllocateUnderlying(SizeOf<T>(), length);
        }

        public HugeManagedArray(T[] copyFrom)
            : this(copyFrom.ValidateNotNull(nameof(copyFrom)).LongLength)
        {
            if (this.Length != 0)
            {
                DangerousCopyCore(sourceStart: ref copyFrom[0],
                                  destinationStart: ref this.GetRefForValidatedIndex(0),
                                  length: this.Length);
            }
        }

        public HugeManagedArray(HugeManagedArray<T> copyFrom)
        {
            copyFrom.ValidateNotNull(nameof(copyFrom));
            this.blocks = new HugeManagedArrayBlock[copyFrom.blocks.Length];
            new ReadOnlySpan<HugeManagedArrayBlock>(copyFrom.blocks).CopyTo(this.blocks);
            this.Length = copyFrom.Length;
        }

        public long Length { get; }

        public ref T this[long index] => ref this.GetRefForValidatedIndex(index.ValidateInRange(nameof(index), 0, this.Length));

        public HugeManagedArray<T> Clone() => new HugeManagedArray<T>(this);

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => new Enumerator(this);

        // TODO: add resizing capability and unlimited slicing capabilities

        public Span<T> Slice(long index, int length)
        {
            index.ValidateInRange(nameof(index), 0, this.Length);
            ((long)length).ValidateInRange(nameof(length), 0, this.Length - index + 1);
            return Span<T>.DangerousCreate(this.blocks, ref this.GetRefForValidatedIndex(index), length);
        }

        // CopyTo: base our overloads off of the Array.Copy overloads.
        public void CopyTo(HugeManagedArray<T> destinationArray, long length)
        {
            destinationArray.ValidateNotNull(nameof(destinationArray));
            length.ValidateInRange(nameof(length), 0, this.Length + 1);
            length.ValidateInRange(nameof(length), 0, destinationArray.Length + 1);
            this.CopyToCore(0, destinationArray, 0, length);
        }

        public void CopyTo(long sourceIndex, HugeManagedArray<T> destinationArray, long destinationIndex, long length)
        {
            destinationArray.ValidateNotNull(nameof(destinationArray));
            sourceIndex.ValidateInRange(nameof(sourceIndex), 0, this.Length);
            destinationIndex.ValidateInRange(nameof(destinationIndex), 0, destinationArray.Length);
            length.ValidateInRange(nameof(length), 0, this.Length - sourceIndex + 1);
            length.ValidateInRange(nameof(length), 0, destinationArray.Length - destinationIndex + 1);
            this.CopyToCore(sourceIndex, destinationArray, destinationIndex, length);
        }

        private void CopyToCore(long sourceIndex, HugeManagedArray<T> destinationArray, long destinationIndex, long length) => this.DangerousCopyToCore(sourceIndex, ref destinationArray.GetRefForValidatedIndex(destinationIndex), length);

        // DangerousCopyTo: dangerous because we can't prevent overrunning the destination buffer.
        public void DangerousCopyTo(ref T destinationStart) =>
            this.DangerousCopyToCore(sourceIndex: 0,
                                     destinationStart: ref destinationStart,
                                     length: this.Length);

        public void DangerousCopyTo(long sourceIndex, ref T start) =>
            this.DangerousCopyToCore(sourceIndex: sourceIndex.ValidateInRange(nameof(sourceIndex), 0, this.Length),
                                     destinationStart: ref start,
                                     length: this.Length - sourceIndex);

        public void DangerousCopyTo(long sourceIndex, ref T destinationStart, long length) =>
            this.DangerousCopyToCore(sourceIndex: sourceIndex.ValidateInRange(nameof(sourceIndex), 0, this.Length),
                                     destinationStart: ref destinationStart,
                                     length: length.ValidateInRange(nameof(length), 0, this.Length - sourceIndex + 1));

        private unsafe void DangerousCopyToCore(long sourceIndex, ref T destinationStart, long length) =>
            DangerousCopyCore(sourceStart: ref this.GetRefForValidatedIndex(sourceIndex),
                              destinationStart: ref destinationStart,
                              length: length);

        private static unsafe void DangerousCopyCore(ref T sourceStart, ref T destinationStart, long length)
        {
            if (length == 0)
            {
                return;
            }

            long byteCount = length * SizeOf<T>();

            fixed (void* src = &As<T, byte>(ref sourceStart))
            fixed (void* dst = &As<T, byte>(ref destinationStart))
            {
                Buffer.MemoryCopy(src, dst, byteCount, byteCount);
            }
        }

        private ref T GetRefForValidatedIndex(long index) => ref Add(ref As<HugeManagedArrayBlock, T>(ref this.blocks[0]), new IntPtr(index));

        // TODO: remove after we can use the following to tell the compiler to do this for us:
        // https://github.com/dotnet/csharplang/blob/master/proposals/blittable.md
        private void ValidateBlittable()
        {
            if (HugeManagedArray.ContainsReferences<T>())
            {
                ThrowHelpers.ThrowExceptionForNonBlittable();
            }
        }

        public struct Enumerator : IEnumerator<T>
        {
            private HugeManagedArray<T> array;

            private long index;

            internal Enumerator(HugeManagedArray<T> array)
            {
                this.array = array;
                this.index = -1;
            }

            public bool MoveNext() => this.index < this.array.Length &&
                                      ++this.index < this.array.Length;

            public T Current => this.array[this.index];
            object System.Collections.IEnumerator.Current => this.array[this.index];

            public void Reset() => this.index = -1;

            void IDisposable.Dispose() { }
        }
    }
}
