using System.Buffers;

namespace AirBreather
{
    public sealed class ByteArrayPool
    {
        private readonly ArrayPool<byte> underlying = ArrayPool<byte>.Shared;

        private ByteArrayPool() { }

        public static ByteArrayPool Instance { get; } = new ByteArrayPool();

        public byte[] Rent(int minimumLength) => this.underlying.Rent(minimumLength);

        public void Return(byte[] buffer, bool clearArray = false) => this.underlying.Return(buffer, clearArray);
    }
}
