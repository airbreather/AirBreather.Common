using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AirBreather.IO
{
    public delegate void FieldProcessedEventHandler(object sender, ReadOnlySpan<byte> data);

    public class Rfc4180CsvHelper
    {
        public const int DefaultMaxFieldLength = 1024 * 1024;

        public const int DefaultMinReadBufferLength = 65536;

        private const byte COMMA = (byte)',';

        private const byte CR = (byte)'\r';

        private const byte LF = (byte)'\n';

        private const byte QUOTE = (byte)'"';

        private static readonly byte[] UnquotedStopBytes = { COMMA, QUOTE, CR, LF };

        private int maxFieldLength = DefaultMaxFieldLength;

        private int minReadBufferLength = DefaultMinReadBufferLength;

        public int MaxFieldLength
        {
            get => this.maxFieldLength;
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Must be greater than zero.");
                }

                this.maxFieldLength = value;
            }
        }

        public int MinReadBufferLength
        {
            get => this.minReadBufferLength;
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Must be greater than zero.");
                }

                this.minReadBufferLength = value;
            }
        }

        public ArrayPool<byte> BufferPool { get; set; }

        public event FieldProcessedEventHandler FieldProcessed;

        public event EventHandler EndOfLine;

        public async Task ReadUtf8CsvFileAsync(Stream stream, IProgress<int> progress = null, CancellationToken cancellationToken = default)
        {
            stream.ValidateNotNull(nameof(stream));

            var bufferPool = this.BufferPool ?? ArrayPool<byte>.Shared;

            byte[] readBuffer = null;
            byte[] fieldBuffer = null;
            try
            {
                readBuffer = bufferPool.Rent(this.minReadBufferLength);
                fieldBuffer = bufferPool.Rent(this.maxFieldLength);
                var sizedFieldBuffer = new ArraySegment<byte>(fieldBuffer, 0, this.maxFieldLength);

                int fieldBufferConsumed = 0;
                bool alwaysEmitLastField = false;
                bool fieldIsQuoted = false;
                bool escapeNextQuote = false;
                bool ignoreNextLinefeed = false;

                int lastRead;
                while ((lastRead = await stream.ReadAsync(readBuffer, 0, readBuffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
                {
                    this.ProcessFields(new ReadOnlySpan<byte>(readBuffer, 0, lastRead), sizedFieldBuffer, ref fieldBufferConsumed, ref alwaysEmitLastField, ref fieldIsQuoted, ref escapeNextQuote, ref ignoreNextLinefeed);
                    progress?.Report(lastRead);
                }

                this.ProcessEndOfFields(new ReadOnlySpan<byte>(fieldBuffer, 0, fieldBufferConsumed), alwaysEmitLastField);
                progress?.Report(0);
            }
            finally
            {
                if (readBuffer != null)
                {
                    bufferPool.Return(readBuffer, clearArray: true);
                }

                if (fieldBuffer != null)
                {
                    bufferPool.Return(fieldBuffer, clearArray: true);
                }
            }
        }

        private void ProcessFields(ReadOnlySpan<byte> readBuffer, Span<byte> fieldBuffer, ref int fieldBufferConsumed, ref bool alwaysEmitLastField, ref bool fieldIsQuoted, ref bool escapeNextQuote, ref bool ignoreNextLinefeed)
        {
            Debug.Assert(fieldBuffer.Length == this.maxFieldLength);

            var unquotedStopBytes = new ReadOnlySpan<byte>(UnquotedStopBytes);
            var freeFieldBuffer = fieldBuffer.Slice(fieldBufferConsumed);

            while (!readBuffer.IsEmpty)
            {
                int idx;
                bool stoppedOnEscapedQuote = false;
                if (fieldIsQuoted && !escapeNextQuote)
                {
                    idx = readBuffer.IndexOf(QUOTE);
                }
                else
                {
                    idx = readBuffer.IndexOfAny(unquotedStopBytes);
                    if (escapeNextQuote)
                    {
                        if (idx == 0 && readBuffer[0] == QUOTE)
                        {
                            stoppedOnEscapedQuote = true;
                        }
                        else
                        {
                            fieldIsQuoted = false;
                        }

                        escapeNextQuote = false;
                    }
                }

                byte stopByte = 0;
                ReadOnlySpan<byte> copyChunk;
                if (idx < 0)
                {
                    copyChunk = readBuffer;
                    readBuffer = default;
                }
                else if (stoppedOnEscapedQuote)
                {
                    copyChunk = readBuffer.Slice(0, 1);
                    readBuffer = readBuffer.Slice(1);
                }
                else
                {
                    stopByte = readBuffer[idx];
                    copyChunk = readBuffer.Slice(0, idx);
                    readBuffer = readBuffer.Slice(idx + 1);
                }

                if (copyChunk.Length > freeFieldBuffer.Length)
                {
                    FieldTooLong();
                }

                if (!copyChunk.IsEmpty)
                {
                    copyChunk.CopyTo(freeFieldBuffer.Slice(0, copyChunk.Length));
                    freeFieldBuffer = freeFieldBuffer.Slice(copyChunk.Length);
                }

                if (ignoreNextLinefeed)
                {
                    if (stopByte == LF)
                    {
                        stopByte = 0;
                    }

                    ignoreNextLinefeed = false;
                }

                switch (stopByte)
                {
                    case QUOTE:
                        if (fieldIsQuoted)
                        {
                            escapeNextQuote = true;
                        }
                        else
                        {
                            fieldIsQuoted = true;
                        }

                        break;

                    case COMMA:
                        this.FieldProcessed?.Invoke(this, fieldBuffer.Slice(0, fieldBuffer.Length - freeFieldBuffer.Length));
                        freeFieldBuffer = fieldBuffer;
                        alwaysEmitLastField = true;
                        break;

                    case CR:
                        ignoreNextLinefeed = true;
                        goto case LF;

                    case LF:
                        this.ProcessEndOfFields(fieldBuffer.Slice(0, fieldBuffer.Length - freeFieldBuffer.Length), alwaysEmitLastField);
                        freeFieldBuffer = fieldBuffer;
                        alwaysEmitLastField = false;
                        break;
                }
            }

            fieldBufferConsumed = fieldBuffer.Length - freeFieldBuffer.Length;
        }

        private void ProcessEndOfFields(ReadOnlySpan<byte> lastFieldData, bool alwaysEmitLastField)
        {
            if (!lastFieldData.IsEmpty || alwaysEmitLastField)
            {
                this.FieldProcessed?.Invoke(this, lastFieldData);
            }

            this.EndOfLine?.Invoke(this, EventArgs.Empty);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void FieldTooLong() => throw new InvalidDataException($"Data contains one or more fields that exceed the limit set in {nameof(MaxFieldLength)}.");
    }
}
