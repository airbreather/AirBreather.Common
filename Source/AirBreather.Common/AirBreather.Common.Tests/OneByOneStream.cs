using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AirBreather.Tests
{
    internal sealed class OneByOneStream : Stream
    {
        private readonly Stream _wrapped;

        public OneByOneStream(Stream wrapped) => _wrapped = wrapped ?? throw new ArgumentNullException(nameof(wrapped));

        public override bool CanRead => _wrapped.CanRead;

        public override bool CanSeek => _wrapped.CanSeek;

        public override bool CanTimeout => _wrapped.CanTimeout;

        public override bool CanWrite => _wrapped.CanWrite;

        public override long Length => _wrapped.Length;

        public override long Position
        {
            get => _wrapped.Position;
            set => _wrapped.Position = value;
        }

        public override int ReadTimeout
        {
            get => _wrapped.ReadTimeout;
            set => _wrapped.ReadTimeout = value;
        }

        public override int WriteTimeout
        {
            get => _wrapped.WriteTimeout;
            set => _wrapped.WriteTimeout = value;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => _wrapped.BeginRead(buffer, offset, 1, callback, state);

        public override int EndRead(IAsyncResult asyncResult) => _wrapped.EndRead(asyncResult);

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            var tcs = new TaskCompletionSource<bool>(state);

            Task.Run(async () =>
            {
                try
                {
                    for (int i = 0; i < count; i++)
                    {
                        await Task.Factory.FromAsync(_wrapped.BeginWrite, _wrapped.EndWrite, buffer, i, 1, null).ConfigureAwait(false);
                    }

                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        public override void EndWrite(IAsyncResult asyncResult) => ((Task)asyncResult).GetAwaiter().GetResult();

        public override void Close() => _wrapped.Close();

        public override void CopyTo(Stream destination, int bufferSize) => _wrapped.CopyTo(destination, bufferSize);

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken = default) => _wrapped.CopyToAsync(destination, bufferSize, cancellationToken);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _wrapped.Dispose();
            }
        }

        public override void Flush() => _wrapped.Flush();

        public override Task FlushAsync(CancellationToken cancellationToken = default) => _wrapped.FlushAsync(cancellationToken);

        public override int Read(Span<byte> buffer) => _wrapped.Read(buffer.Slice(0, 1));

        public override int Read(byte[] buffer, int offset, int count) => _wrapped.Read(buffer, offset, 1);

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => _wrapped.ReadAsync(buffer.Slice(0, 1), cancellationToken);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) => _wrapped.ReadAsync(buffer, offset, 1, cancellationToken);

        public override int ReadByte() => _wrapped.ReadByte();

        public override long Seek(long offset, SeekOrigin origin) => _wrapped.Seek(offset, origin);

        public override void SetLength(long value) => _wrapped.SetLength(value);

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                _wrapped.Write(buffer.Slice(i, 1));
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                _wrapped.Write(buffer, offset + i, 1);
            }
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            for (int i = 0; i < count; i++)
            {
                await _wrapped.WriteAsync(buffer, offset + i, 1, cancellationToken).ConfigureAwait(false);
            }
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                await _wrapped.WriteAsync(buffer.Slice(i, 1), cancellationToken).ConfigureAwait(false);
            }
        }

        public override void WriteByte(byte value) => _wrapped.WriteByte(value);
    }
}
