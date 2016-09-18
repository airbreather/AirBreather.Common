using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AirBreather
{
    public class ReadThrottledStream : Stream
    {
        private static readonly double TimeSpanTicksPerStopwatchTick = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        private readonly Stream innerStream;
        private readonly long bytesPerSecond;

        private Task throttleTask = Task.CompletedTask;

        public ReadThrottledStream(Stream innerStream, long bytesPerSecond)
        {
            this.innerStream = innerStream.ValidateNotNull(nameof(innerStream));
            if (!this.innerStream.CanRead)
            {
                throw new ArgumentException("Must support reading.", nameof(innerStream));
            }

            this.bytesPerSecond = bytesPerSecond.ValidateNotLessThan(nameof(bytesPerSecond), 1);
        }

        public override bool CanRead => true;
        public override bool CanWrite => false;
        public override bool CanSeek => this.innerStream.CanSeek;

        public override long Length => this.innerStream.Length;
        public override long Position
        {
            get { return this.innerStream.Position; }
            set { this.innerStream.Position = value; }
        }

        public override void Flush() => this.innerStream.Flush();
        public override long Seek(long offset, SeekOrigin origin) => this.innerStream.Seek(offset, origin);
        public override void SetLength(long value) => this.WriteOperationsNotSupported();
        public override void Write(byte[] buffer, int offset, int count) => this.WriteOperationsNotSupported();

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.throttleTask.Wait();
            long startTime = Stopwatch.GetTimestamp();
            count = this.innerStream.Read(buffer, offset, count);

            this.SetupThrottleTaskForNextTime(count, startTime);
            return count;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            Task waitTask = this.throttleTask;
            if (!this.throttleTask.IsCompleted && cancellationToken.CanBeCanceled)
            {
                waitTask = cancellationToken.IsCancellationRequested
                    ? Task.FromCanceled(cancellationToken)
                    : Task.WhenAny(this.throttleTask, cancellationToken.WaitHandle.WaitOneAsync());
            }

            await waitTask.ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            long startTime = Stopwatch.GetTimestamp();

            // possible alternative: start both a full-delay task and this task at the same time,
            // set throttleTask to Task.WhenAll on both of them, and then just await our reader.
            // this approach seems more efficient, though, especially since we might read less than
            // what was requested.
            count = await this.innerStream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);

            this.SetupThrottleTaskForNextTime(count, startTime);
            return count;
        }

        // we don't consider this class to "own" its inner stream, which is why we don't do this:
#if false
        public override void Close()
        {
            this.innerStream.Close();
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.innerStream.Dispose();
            }

            base.Dispose(disposing);
        }
#endif

        private void WriteOperationsNotSupported()
        {
            throw new NotSupportedException("The stream does not support writing.");
        }

        private void SetupThrottleTaskForNextTime(int count, long startTimestamp)
        {
            this.throttleTask = Task.CompletedTask;
            if (count < 1)
            {
                return;
            }

            long target = ((long)(((double)count / this.bytesPerSecond) * Stopwatch.Frequency)) + startTimestamp;
            if (Stopwatch.GetTimestamp() < target)
            {
                this.throttleTask = TaskUtilityExperimental.PreciseDelay(target);
            }
        }
    }
}
