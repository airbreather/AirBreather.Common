using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AirBreather.IO
{
    using static DecimalSerializationUtility;

    public sealed class AsyncBinaryReader : BinaryReader
    {
        // largest value type to use this is System.Decimal, which is 16 bytes.
        private byte[] buffer = new byte[16];

        public AsyncBinaryReader(Stream input) : base(input)
        {
        }

        public AsyncBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        public AsyncBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        public Task<bool> ReadBooleanAsync() => this.ReadBooleanAsync(CancellationToken.None);
        public async Task<bool> ReadBooleanAsync(CancellationToken cancellationToken) => await this.ReadValueAsync<byte>(cancellationToken).ConfigureAwait(false) != 0;

        public Task<byte> ReadByteAsync() => this.ReadValueAsync<byte>(CancellationToken.None);
        public Task<byte> ReadByteAsync(CancellationToken cancellationToken) => this.ReadValueAsync<byte>(cancellationToken);

        public Task<sbyte> ReadSByteAsync() => this.ReadValueAsync<sbyte>(CancellationToken.None);
        public Task<sbyte> ReadSByteAsync(CancellationToken cancellationToken) => this.ReadValueAsync<sbyte>(cancellationToken);

        public Task<short> ReadInt16Async() => this.ReadValueAsync<short>(CancellationToken.None);
        public Task<short> ReadInt16Async(CancellationToken cancellationToken) => this.ReadValueAsync<short>(cancellationToken);

        public Task<ushort> ReadUInt16Async() => this.ReadValueAsync<ushort>(CancellationToken.None);
        public Task<ushort> ReadUInt16Async(CancellationToken cancellationToken) => this.ReadValueAsync<ushort>(cancellationToken);

        public Task<int> ReadInt32Async() => this.ReadValueAsync<int>(CancellationToken.None);
        public Task<int> ReadInt32Async(CancellationToken cancellationToken) => this.ReadValueAsync<int>(cancellationToken);

        public Task<uint> ReadUInt32Async() => this.ReadValueAsync<uint>(CancellationToken.None);
        public Task<uint> ReadUInt32Async(CancellationToken cancellationToken) => this.ReadValueAsync<uint>(cancellationToken);

        public Task<long> ReadInt64Async() => this.ReadValueAsync<long>(CancellationToken.None);
        public Task<long> ReadInt64Async(CancellationToken cancellationToken) => this.ReadValueAsync<long>(cancellationToken);

        public Task<ulong> ReadUInt64Async() => this.ReadValueAsync<ulong>(CancellationToken.None);
        public Task<ulong> ReadUInt64Async(CancellationToken cancellationToken) => this.ReadValueAsync<ulong>(cancellationToken);

        public Task<float> ReadSingleAsync() => this.ReadValueAsync<float>(CancellationToken.None);
        public Task<float> ReadSingleAsync(CancellationToken cancellationToken) => this.ReadValueAsync<float>(cancellationToken);

        public Task<double> ReadDoubleAsync() => this.ReadValueAsync<double>(CancellationToken.None);
        public Task<double> ReadDoubleAsync(CancellationToken cancellationToken) => this.ReadValueAsync<double>(cancellationToken);

        public Task<decimal> ReadDecimalAsync() => this.ReadDecimalAsync(CancellationToken.None);
        public async Task<decimal> ReadDecimalAsync(CancellationToken cancellationToken)
        {
            await this.FillBufferAsync(16, cancellationToken).ConfigureAwait(false);

            decimal d0 = default(decimal);
            dFlags(ref d0) = wFlags(ref this.buffer[0]);
            dHi(ref d0) = wHi(ref this.buffer[0]);
            dLo(ref d0) = wLo(ref this.buffer[0]);
            dMid(ref d0) = wMid(ref this.buffer[0]);
            return d0;
        }

        public Task<int> ReadAsync(byte[] buffer, int index, int count) => this.BaseStream.ReadAsync(buffer, index, count);
        public Task<int> ReadAsync(byte[] buffer, int index, int count, CancellationToken cancellationToken) => this.BaseStream.ReadAsync(buffer, index, count, cancellationToken);

        public Task<byte[]> ReadBytesAsync(int count) => this.ReadBytesAsync(count, CancellationToken.None);
        public async Task<byte[]> ReadBytesAsync(int count, CancellationToken cancellationToken)
        {
            byte[] result = new byte[count];
            if (result.Length != (count = await this.BaseStream.LoopedReadAsync(result, 0, count).ConfigureAwait(false)))
            {
                Array.Resize(ref result, count);
            }

            return result;
        }

        private async Task<T> ReadValueAsync<T>(CancellationToken cancellationToken)
        {
            await this.FillBufferAsync(Unsafe.SizeOf<T>(), cancellationToken).ConfigureAwait(false);
            return Unsafe.As<byte, T>(ref this.buffer[0]);
        }

        private async Task FillBufferAsync(int cnt, CancellationToken cancellationToken)
        {
            int off = 0;
            int prev = 0;
            do
            {
                prev = await this.BaseStream.ReadAsync(this.buffer, off, cnt, cancellationToken).ConfigureAwait(false);
                cnt -= prev;
                off += prev;
            }
            while ((cnt != 0) & (prev != 0));

            if (cnt != 0)
            {
                throw new EndOfStreamException();
            }
        }
    }
}
