using System;
using System.Collections.Generic;
using System.Linq;

namespace AirBreather.Text
{
    public struct Windows1252String : IEquatable<Windows1252String>, IEnumerable<char>
    {
        public Windows1252String(string clrString) => this.EncodedData = new ArraySegment<byte>(EncodingEx.Windows1252.GetBytes(clrString));

        public Windows1252String(Windows1252String copyFrom) => this.EncodedData = new ArraySegment<byte>(copyFrom.EncodedData.ToArray());

        private Windows1252String(ArraySegment<byte> encodedData) => this.EncodedData = encodedData;

        public ArraySegment<byte> EncodedData { get; }

        public int Length => this.EncodedData.Count;

        public static implicit operator string(Windows1252String Windows1252String) => EncodingEx.Windows1252.GetString(Windows1252String.EncodedData.Array, Windows1252String.EncodedData.Offset, Windows1252String.EncodedData.Count);

        public static implicit operator Windows1252String(string clrString) => new Windows1252String(clrString);

        public static Windows1252String Wrap(ArraySegment<byte> encodedData) => new Windows1252String(encodedData);

        public static Windows1252String CreateRented(string clrString, ByteArrayPool pool = null)
        {
            pool = pool ?? ByteArrayPool.Instance;
            byte[] buffer = pool.Rent(clrString.Length);
            try
            {
                EncodingEx.Windows1252.GetBytes(clrString, 0, clrString.Length, buffer, 0);
                return new Windows1252String(new ArraySegment<byte>(buffer));
            }
            catch
            {
                pool.Return(buffer);
                throw;
            }
        }

        public void ReturnRentedBuffer(ByteArrayPool pool = null) => (pool ?? ByteArrayPool.Instance).Return(this.EncodedData.Array);

        public override string ToString() => this;

        public override bool Equals(object obj) => obj is Windows1252String other && this.Equals(other);

        public bool Equals(Windows1252String other) => ((ReadOnlySpan<byte>)this.EncodedData).EqualsData(other.EncodedData);

        public override int GetHashCode() => this.EncodedData.Murmur3_32();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

        public IEnumerator<char> GetEnumerator() => Helpers.CreateCharEnumerator(this.EncodedData, EncodingEx.Windows1252);
    }
}
