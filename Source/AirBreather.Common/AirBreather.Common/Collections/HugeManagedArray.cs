using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static System.Runtime.CompilerServices.Unsafe;

namespace AirBreather.Collections
{
    internal static class HugeManagedArray
    {
        internal static HugeManagedArrayBlock[] AllocateUnderlying(int sz, long length) =>
            new HugeManagedArrayBlock[Math.DivRem(sz * length, SizeOf<HugeManagedArrayBlock>(), out long rem) + (rem == 0 ? 0 : 1)];
    }

    // at this block size, with <gcAllowVeryLargeObjects> enabled, a single array can hold up to
    // just barely under 128 TB of data (65532 * 0x7FEFFFFF bytes), provided enough virtual memory.
    [StructLayout(LayoutKind.Sequential, Size = 65532)]
    internal struct HugeManagedArrayBlock { }

    public sealed class HugeManagedArray<T> : IEnumerable<T>
        where T : unmanaged
    {
        private readonly HugeManagedArrayBlock[] blocks;

        public HugeManagedArray(long length)
        {
            this.Length = length.ValidateNotLessThan(nameof(length), 0);
            this.blocks = length == 0
                ? Array.Empty<HugeManagedArrayBlock>()
                : HugeManagedArray.AllocateUnderlying(SizeOf<T>(), length);
        }

        public HugeManagedArray(Span<T> copyFrom)
            : this(copyFrom.Length)
        {
            if (this.Length != 0)
            {
                Span<T> copyTo = MemoryMarshal.Cast<HugeManagedArrayBlock, T>(this.blocks).Slice(0, copyFrom.Length);
                copyFrom.CopyTo(copyTo);
            }
        }

        public HugeManagedArray(HugeManagedArray<T> copyFrom)
        {
            copyFrom.ValidateNotNull(nameof(copyFrom));
            if (copyFrom.Length == 0)
            {
                this.blocks = Array.Empty<HugeManagedArrayBlock>();
            }
            else
            {
                this.Length = copyFrom.Length;
                this.blocks = new HugeManagedArrayBlock[copyFrom.blocks.Length];
                copyFrom.blocks.CopyTo(this.blocks);
            }
        }

        public long Length { get; }

        public ref T this[long index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (unchecked(((ulong)index) >= (ulong)this.Length))
                {
                    ThrowHelpers.ThrowArgumentOutOfRangeException_Index_CollectionLength();
                }

                return ref this.GetRefForValidatedIndex(index);
            }
        }

        public HugeManagedArray<T> Clone() => new HugeManagedArray<T>(this);

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => new Enumerator(this);

        // TODO: add resizing and slicing capabilities

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

        private unsafe void CopyToCore(long sourceIndex, HugeManagedArray<T> destinationArray, long destinationIndex, long length)
        {
            if (length == 0)
            {
                return;
            }

            // TODO: copy individual spans, or ReadOnlySequence<T>?  pinning feels bad.
            // .NET Core targets have MemoryMarshal.Create{ReadOnly,}Span<T> that we could use too.
            fixed (T* src = &this.GetRefForValidatedIndex(sourceIndex))
            fixed (T* dst = &destinationArray.GetRefForValidatedIndex(destinationIndex))
            {
                Buffer.MemoryCopy(src, dst, length, length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T GetRefForValidatedIndex(long index) =>
            ref Add(ref As<HugeManagedArrayBlock, T>(ref this.blocks[0]), new IntPtr(index));

        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<T>
        {
#pragma warning disable IDE0044
            private HugeManagedArray<T> array;
#pragma warning restore IDE0044

            private long index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(HugeManagedArray<T> array)
            {
                this.array = array;
                this.index = -1;
            }

            public ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref this.array.GetRefForValidatedIndex(this.index);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                long newIndex = this.index + 1;
                if (newIndex < this.array.Length)
                {
                    this.index = newIndex;
                    return true;
                }

                return false;
            }

            T IEnumerator<T>.Current => this.Current;
            object System.Collections.IEnumerator.Current
            {
                get
                {
                    // IEnumerator.get_Current is documented as needing to throw when accessed after
                    // MoveNext() has returned false.
                    if (unchecked(((ulong)this.index) >= (ulong)this.array.Length))
                    {
                        ThrowHelpers.ThrowInvaildOperationExceptionForUnexpectedCurrent();
                    }

                    return this.Current;
                }
            }

            void System.Collections.IEnumerator.Reset() => this.index = -1;
            void IDisposable.Dispose() { }
        }
    }
}
