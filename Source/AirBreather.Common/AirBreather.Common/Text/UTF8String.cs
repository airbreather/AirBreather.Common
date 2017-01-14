using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AirBreather.Text
{
    // reasons to prefer this over CLR strings:
    // - a separate object on the heap for every single string is unacceptable
    // - you need the UTF-8 encoded representation regularly enough
    // - you have long strings of characters mostly in the ASCII range, in unlikely circumstances
    // reasons not to prefer this over CLR strings:
    // - size, sometimes (fixed 8 byte cost per string, also some very specific code unit sequences)
    // - speed and/or portability, since most APIs use CLR strings which have to be copied
    // ideally, we'd use the corefxlab type of the same name, but Span<T> isn't available on all
    // runtimes that I care about... yet.
    public struct Utf8String : IEquatable<Utf8String>, IEnumerable<char>
    {
        public Utf8String(string clrString) => this.EncodedData = new ArraySegment<byte>(EncodingEx.UTF8NoBOM.GetBytes(clrString));

        public Utf8String(Utf8String copyFrom) => this.EncodedData = new ArraySegment<byte>(copyFrom.EncodedData.ToArray());

        public Utf8String(ArraySegment<byte> encodedData) => this.EncodedData = encodedData;

        public ArraySegment<byte> EncodedData { get; }

        public int Length => EncodingEx.UTF8NoBOM.GetCharCount(this.EncodedData.Array, this.EncodedData.Offset, this.EncodedData.Count);

        public static implicit operator string(Utf8String utf8String) => EncodingEx.UTF8NoBOM.GetString(utf8String.EncodedData.Array, utf8String.EncodedData.Offset, utf8String.EncodedData.Count);

        public static implicit operator Utf8String(string clrString) => new Utf8String(clrString);

        public static Utf8String CreateRented(string clrString, ByteArrayPool pool = null)
        {
            pool = pool ?? ByteArrayPool.Instance;
            int byteCount = EncodingEx.UTF8NoBOM.GetByteCount(clrString);
            byte[] buffer = pool.Rent(byteCount);
            try
            {
                EncodingEx.UTF8NoBOM.GetBytes(clrString, 0, clrString.Length, buffer, 0);
                return new Utf8String(new ArraySegment<byte>(buffer, 0, byteCount));
            }
            catch
            {
                pool.Return(buffer);
                throw;
            }
        }

        public void ReturnRentedBuffer(ByteArrayPool pool = null) => (pool ?? ByteArrayPool.Instance).Return(this.EncodedData.Array);

        public override string ToString() => this;

        public override bool Equals(object obj) => obj is Utf8String other && this.Equals(other);

        public bool Equals(Utf8String other) => this.EncodedData.EqualsData(other.EncodedData);

        public override int GetHashCode() => this.EncodedData.Murmur3_32();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

        public IEnumerator<char> GetEnumerator() => Helpers.CreateCharEnumerator(this.EncodedData, EncodingEx.UTF8NoBOM);
    }
}
