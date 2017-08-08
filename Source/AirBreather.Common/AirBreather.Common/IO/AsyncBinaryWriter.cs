using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AirBreather.IO
{
    using static DecimalSerializationUtility;

    public sealed class AsyncBinaryWriter : BinaryWriter
    {
        // largest value type to use this is System.Decimal, which is 16 bytes.
        private readonly byte[] buffer = new byte[16];

        public AsyncBinaryWriter(Stream output) : base(output)
        {
        }

        public AsyncBinaryWriter(Stream output, Encoding encoding) : base(output, encoding)
        {
        }

        public AsyncBinaryWriter(Stream output, Encoding encoding, bool leaveOpen) : base(output, encoding, leaveOpen)
        {
        }

        public Task WriteAsync(bool value) => this.WriteAsyncCore(value ? (byte)1 : (byte)0, CancellationToken.None);
        public Task WriteAsync(bool value, CancellationToken cancellationToken) => this.WriteAsyncCore(value ? (byte)1 : (byte)0, cancellationToken);

        public Task WriteAsync(byte value) => this.WriteAsyncCore(value, CancellationToken.None);
        public Task WriteAsync(byte value, CancellationToken cancellationToken) => this.WriteAsyncCore(value, cancellationToken);

        public Task WriteAsync(sbyte value) => this.WriteAsyncCore(value, CancellationToken.None);
        public Task WriteAsync(sbyte value, CancellationToken cancellationToken) => this.WriteAsyncCore(value, cancellationToken);

        public Task WriteAsync(short value) => this.WriteAsyncCore(value, CancellationToken.None);
        public Task WriteAsync(short value, CancellationToken cancellationToken) => this.WriteAsyncCore(value, cancellationToken);

        public Task WriteAsync(ushort value) => this.WriteAsyncCore(value, CancellationToken.None);
        public Task WriteAsync(ushort value, CancellationToken cancellationToken) => this.WriteAsyncCore(value, cancellationToken);

        public Task WriteAsync(int value) => this.WriteAsyncCore(value, CancellationToken.None);
        public Task WriteAsync(int value, CancellationToken cancellationToken) => this.WriteAsyncCore(value, cancellationToken);

        public Task WriteAsync(uint value) => this.WriteAsyncCore(value, CancellationToken.None);
        public Task WriteAsync(uint value, CancellationToken cancellationToken) => this.WriteAsyncCore(value, cancellationToken);

        public Task WriteAsync(long value) => this.WriteAsyncCore(value, CancellationToken.None);
        public Task WriteAsync(long value, CancellationToken cancellationToken) => this.WriteAsyncCore(value, cancellationToken);

        public Task WriteAsync(ulong value) => this.WriteAsyncCore(value, CancellationToken.None);
        public Task WriteAsync(ulong value, CancellationToken cancellationToken) => this.WriteAsyncCore(value, cancellationToken);

        public Task WriteAsync(float value) => this.WriteAsyncCore(value, CancellationToken.None);
        public Task WriteAsync(float value, CancellationToken cancellationToken) => this.WriteAsyncCore(value, cancellationToken);

        public Task WriteAsync(double value) => this.WriteAsyncCore(value, CancellationToken.None);
        public Task WriteAsync(double value, CancellationToken cancellationToken) => this.WriteAsyncCore(value, cancellationToken);

        public Task WriteAsync(decimal value) => this.WriteAsync(value, CancellationToken.None);
        public Task WriteAsync(decimal value, CancellationToken cancellationToken)
        {
            wFlags(ref this.buffer[0]) = dFlags(ref value);
            wHi(ref this.buffer[0]) = dHi(ref value);
            wLo(ref this.buffer[0]) = dLo(ref value);
            wMid(ref this.buffer[0]) = dMid(ref value);
            return this.BaseStream.WriteAsync(this.buffer, 0, 16, cancellationToken);
        }

        private Task WriteAsyncCore<T>(T value, CancellationToken cancellationToken)
        {
            Unsafe.As<byte, T>(ref this.buffer[0]) = value;
            return this.BaseStream.WriteAsync(this.buffer, 0, Unsafe.SizeOf<T>(), cancellationToken);
        }
    }
}
