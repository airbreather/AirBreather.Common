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

    public class Rfc4180CsvReader
    {
        public const int DefaultMaxFieldLength = 1024 * 1024;

        public const int DefaultMinReadBufferLength = 65536;

        private const byte COMMA = (byte)',';

        private const byte CR = (byte)'\r';

        private const byte LF = (byte)'\n';

        private const byte QUOTE = (byte)'"';

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

                // readBuffer is allowed to exceed the user-specified length, but fieldBuffer has to
                // be the exact given size as a maximum, no matter what the pool happened to give us
                var sizedFieldBuffer = new ArraySegment<byte>(fieldBuffer, 0, this.maxFieldLength);

                // this is the parser state that we preserve across calls to the inner method.  the
                // inner method makes heavy use of spans, and this method is async, so there needs
                // to be a bit of segregation between the two (dotnet/csharplang#1331).
                int fieldBufferConsumed = 0;
                bool alwaysEmitLastField = false;
                bool fieldIsQuoted = false;
                bool escapeNextQuote = false;

                while (true)
                {
                    int lastRead = await stream.ReadAsync(readBuffer, 0, readBuffer.Length, cancellationToken).ConfigureAwait(false);
                    if (lastRead == 0)
                    {
                        break;
                    }

                    this.ProcessNextReadBufferChunk(new ReadOnlySpan<byte>(readBuffer, 0, lastRead), sizedFieldBuffer, ref fieldBufferConsumed, ref alwaysEmitLastField, ref fieldIsQuoted, ref escapeNextQuote);
                    progress?.Report(lastRead);
                }

                // the inner method has no way of knowing when it sees the last field, and not all
                // text files are terminated by a blank line, so we do (sometimes) need to announce
                // one more field along with its EndOfLine event.
                this.NotifyEndOfLineIfNeeded(new ReadOnlySpan<byte>(fieldBuffer, 0, fieldBufferConsumed), alwaysEmitLastField);
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

        private void ProcessNextReadBufferChunk(ReadOnlySpan<byte> readBuffer, Span<byte> cutFieldBuffer, ref int cutFieldBufferConsumed, ref bool alwaysEmitLastField, ref bool fieldIsQuoted, ref bool prevWasQuoteInQuotedField)
        {
            Debug.Assert(cutFieldBuffer.Length == this.maxFieldLength);

            // when we're not reading a quoted field, these are the only bytes that we need to stop
            // on.  UTF-8 is cool like that.
            ReadOnlySpan<byte> unquotedStopBytes = stackalloc byte[] { COMMA, QUOTE, CR, LF };

            // we're going to consume the entire buffer that was handed to us.  it's possible that a
            // field will be split across multiple reads, which is why we take in cutFieldBuffer: it
            // contains zero or more characters from the end of the last call.
            while (!readBuffer.IsEmpty)
            {
                int idx;
                bool stoppedOnEscapedQuote = false;
                if (fieldIsQuoted && !prevWasQuoteInQuotedField)
                {
                    // we've started reading a quoted field, which means that, AT LEAST, everything
                    // up to the next double-quote is just getting copied straight in.
                    idx = readBuffer.IndexOf(QUOTE);
                }
                else
                {
                    // we're either in the same basic state we started the file in, or we've seen a
                    // double-quote while reading a quoted field and it's possible that the field is
                    // over.  in both cases, we're on the hunt for any of our four special bytes.
                    idx = readBuffer.IndexOfAny(unquotedStopBytes);
                    if (prevWasQuoteInQuotedField)
                    {
                        // we started on the byte immediately after a double-quote that, if left
                        // unescaped, ends the field data; RFC 4180 explicitly says that "Spaces are
                        // considered part of a field and should not be ignored" and that double-
                        // quotes need to surround the entire field (albeit not explicitly), and we
                        // only enter in here if there was at least *something* to read, so the only
                        // legal situation here is for the first byte to be something that advances
                        // the parser state.  anything else is therefore an error.
                        if (idx != 0)
                        {
                            ThrowBadQuotingError();
                        }

                        if (readBuffer[0] == QUOTE)
                        {
                            // the next byte after a double-quote is another double-quote, so we
                            // need to insert a double-quote into the field data and keep the stream
                            // going in "reading quoted field" mode.
                            stoppedOnEscapedQuote = true;
                        }
                        else
                        {
                            // whether we saw a comma or a line-ending character, we're done reading
                            // this quoted field, so reset the flag for next time.
                            fieldIsQuoted = false;
                        }

                        prevWasQuoteInQuotedField = false;
                    }
                }

                if (idx < 0)
                {
                    // the entire remaining buffer lacks any of the bytes that tell us to stop, so
                    // everything in it will be copied verbatim to the cut buffer to ensure that we
                    // don't truncate a field whose data continues the next time we fill the read
                    // buffer from the input stream.
                    if (cutFieldBuffer.Length < cutFieldBufferConsumed + readBuffer.Length)
                    {
                        ThrowFieldTooLongError();
                    }

                    readBuffer.CopyTo(cutFieldBuffer.Slice(cutFieldBufferConsumed, readBuffer.Length));
                    cutFieldBufferConsumed += readBuffer.Length;
                    readBuffer = default;
                    continue;
                }

                if (stoppedOnEscapedQuote)
                {
                    // this special-case is the only time that a stop byte actually needs to show up
                    // in the field data.  handle it specially.
                    if (cutFieldBuffer.Length == cutFieldBufferConsumed)
                    {
                        // the cut buffer can't handle even one more byte.
                        ThrowFieldTooLongError();
                    }

                    cutFieldBuffer[cutFieldBufferConsumed++] = QUOTE;
                    readBuffer = readBuffer.Slice(1);
                    continue;
                }

                byte stopByte = readBuffer[idx];
                ReadOnlySpan<byte> fieldBuffer;
                if (cutFieldBufferConsumed != 0 || fieldIsQuoted)
                {
                    // if there's already field data in the cut buffer, then we can't just use the
                    // field data where it is, because it's incomplete.  so we need to copy what
                    // we've read into the cut buffer and use that as the field buffer.  annoyingly,
                    // the same solution applies to *ALL* quoted fields (for now), because we choose
                    // not to look at another byte to see if the double-quote that we necessarily
                    // stopped on here is actually escaping a double-quote that needs to be written
                    // into the cut buffer no matter what.
                    if (cutFieldBuffer.Length < cutFieldBufferConsumed + idx)
                    {
                        ThrowFieldTooLongError();
                    }

                    readBuffer.Slice(0, idx).CopyTo(cutFieldBuffer.Slice(cutFieldBufferConsumed, idx));
                    cutFieldBufferConsumed += idx;
                    fieldBuffer = cutFieldBuffer.Slice(0, cutFieldBufferConsumed);
                }
                else
                {
                    // good news, everyone: the buffer containing all the data we just read from the
                    // stream can be reused as-is for the field data, no copying required!
                    fieldBuffer = readBuffer.Slice(0, idx);

                    // we could maybe get away with skipping validating the field length, since the
                    // only reasons for the max field length to be configurable all apply to other
                    // situations, but if we skipped it, then some streams could potentially be
                    // considered valid, but then become invalid when more valid rows are added,
                    // because it would start having us cut (and therefore validate) a field that we
                    // wouldn't have cut previously, which would suck.  the default max field length
                    // is very generous, so we validate.
                    if (cutFieldBuffer.Length < fieldBuffer.Length)
                    {
                        ThrowFieldTooLongError();
                    }
                }

                // no matter what we read or what we're going to do with it, we've consumed all the
                // data in the read buffer up to (and, we consider, including) the index at which we
                // stopped.  set it up for the next read.
                readBuffer = readBuffer.Slice(idx + 1);

                // by this point, we've short-circuited out of all special-cases that make us stop
                // on one of these bytes where we should NOT handle them the way that this does.
                switch (stopByte)
                {
                    case QUOTE:
                        if (fieldIsQuoted)
                        {
                            // this is either the escape byte for a literal quote, or the end of a
                            // quoted field.  we can't know without looking at another byte, and the
                            // next byte might only be available after another ReadAsync call, so we
                            // choose to leave it up to the next round to disambiguate.
                            prevWasQuoteInQuotedField = true;
                        }
                        else
                        {
                            fieldIsQuoted = true;

                            // a double-quote may not appear midway through an unquoted field's data
                            if (!fieldBuffer.IsEmpty)
                            {
                                ThrowBadQuotingError();
                            }
                        }

                        break;

                    case COMMA:
                        this.FieldProcessed?.Invoke(this, fieldBuffer);
                        cutFieldBufferConsumed = 0;

                        // ensure that if a comma appears right at the end of a line (or the file),
                        // we still emit an empty field for it.  we need to do this after seeing a
                        // comma, because parsers seem to ignore multiple line endings in a row, but
                        // a line with a comma immediately before the line ending has a blank field
                        // at the end.  so the rule is that any time we see a comma, then the next
                        // line ending will ALWAYS emit one last field.
                        alwaysEmitLastField = true;
                        break;

                    case CR:
                    case LF:
                        this.NotifyEndOfLineIfNeeded(fieldBuffer, alwaysEmitLastField);
                        cutFieldBufferConsumed = 0;

                        // reset the flag after a line ending so that we don't emit any lines with
                        // empty fields if there are multiple line endings in a row.
                        alwaysEmitLastField = false;
                        break;
                }
            }
        }

        private void NotifyEndOfLineIfNeeded(ReadOnlySpan<byte> lastFieldData, bool alwaysEmitLastField)
        {
            if (!lastFieldData.IsEmpty || alwaysEmitLastField)
            {
                this.FieldProcessed?.Invoke(this, lastFieldData);

                // this surprised me, but it appears that most parsers ignore blank lines, even many
                // in a row.  I would have thought that they'd emit lines with no fields, if not the
                // one-empty-field line that we would emit if we removed our conditional, but cool.
                // this does simplify my code a bit, because CRLF is no longer special :-D.
                this.EndOfLine?.Invoke(this, EventArgs.Empty);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowFieldTooLongError() => throw new InvalidDataException($"Data contains one or more fields that exceed the limit set in {nameof(MaxFieldLength)}.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowBadQuotingError() => throw new InvalidDataException("Failed to parse quoted field.");
    }
}
