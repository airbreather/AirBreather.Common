using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AirBreather.Csv
{
    /// <summary>
    /// Reader that processes RFC 4180 (CSV) fields.
    /// </summary>
    /// <remarks>
    /// RFC 4180 leaves a lot of wiggle room for implementers.  The following section explains how
    /// this implementation resolves ambiguities in the spec, explains where and why we deviate from
    /// it, and offers clarifying notes where the spec appears to have "gotchas", in the order that
    /// the relevant items appear in the spec:
    /// <list type="bullet">
    /// <item>
    /// The spec does not specify a maximum field length, but for practical reasons, we do impose a
    /// limit of some sort.  This limit can be controlled by the caller, but we expect that most
    /// streams will be perfectly satisfied leaving it at the default.
    /// </item>
    /// <item>
    /// The spec says that separate lines are delimited by CRLF line breaks.  This implementation
    /// accepts line breaks of any format (CRLF, LF, CR).
    /// </item>
    /// <item>
    /// The spec says that there may or may not be a line break at the end of the last record in the
    /// stream.  This implementation does not require there to be a line break, and it would not
    /// hurt to add one either.
    /// </item>
    /// <item>
    /// The spec refers to an optional header line at the beginning.  This implementation does not
    /// include any special treatment for the first line of fields; if they need to be treated as
    /// headers, then the consumer needs to know that and respond accordingly.
    /// </item>
    /// <item>
    /// The spec says each record may contain "one or more fields".  This implementation interprets
    /// that to mean strictly that any number of consecutive newline characters in a row are treated
    /// as one.
    /// </item>
    /// <item>
    /// Many implementations allow the delimiter character to be configured to be something else
    /// other than a comma.  This implementation does not currently offer that flexibility.
    /// </item>
    /// <item>
    /// Many implementations allow automatically trimming whitespace at the beginning and/or end of
    /// each field (sometimes optionally).  The spec expressly advises against doing that, and this
    /// implementation follows suit.  It is our opinion that consumers ought to be more than capable
    /// of trimming spaces at the beginning or end as part of their processing if this is desired.
    /// </item>
    /// <item>
    /// The spec says that the last field in a record must not be followed by a comma.  This
    /// implementation interprets that to mean that if we do see a comma followed immediately by a
    /// line ending character, then that represents the data for an empty field.
    /// </item>
    /// </list>
    /// <para>
    /// Finally, the spec has a lot to say about double quotes.  This implementation follows the
    /// rules that it expressly lays out, but there are some "gotchas" that follow from the spec
    /// leaving it open-ended how implementations should deal with various streams that include
    /// double quotes which do not completely enclose fields, resolved as follows:
    /// </para>
    /// <para>
    /// If a double quote is encountered at the very beginning of a field, then all characters up
    /// until the next unescaped double quote or the end of the stream (whichever comes first) are
    /// considered to be part of the data for that field (we do translate escaped double quotes for
    /// convenience).  This includes line ending characters, even though Excel seems to only make
    /// that happen if the field counts matching up.  If parsing stopped at an unescaped double
    /// quote, but there are still more bytes after that double quote before the next delimiter,
    /// then all those bytes will be treated verbatim as part of the field's data (double quotes are
    /// no longer special at all for the remainder of the field).
    /// </para>
    /// <para>
    /// Double quotes encountered at any other point are included verbatim as part of the field with
    /// no special processing.
    /// </para>
    /// </remarks>
    public class Rfc4180CsvReader
    {
        /// <summary>
        /// The default value for <see cref="MaxFieldLength"/> (1 MiB).
        /// </summary>
        public static readonly int DefaultMaxFieldLength = 1024 * 1024;

        /// <summary>
        /// The default value for <see cref="MinReadBufferLength"/> (65 KiB).
        /// </summary>
        public static readonly int DefaultMinReadBufferLength = 65536;

        private const byte COMMA = (byte)',';

        private const byte CR = (byte)'\r';

        private const byte LF = (byte)'\n';

        private const byte QUOTE = (byte)'"';

        private int maxFieldLength = DefaultMaxFieldLength;

        private int minReadBufferLength = DefaultMinReadBufferLength;

        /// <summary>
        /// Gets or sets the <see cref="ArrayPool{T}"/> that serves us all the byte arrays that we
        /// ask for, or <see langword="null"/> if we should use <see cref="ArrayPool{T}.Shared"/>.
        /// </summary>
        public ArrayPool<byte> BufferPool { get; set; }

        /// <summary>
        /// Gets or sets the maximum length, in UTF-8 code units (bytes), of the largest field that
        /// this reader can process.  Any individual field longer than this will not be processed.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when trying to set the value to something that is not greater than zero.
        /// </exception>
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

        /// <summary>
        /// Gets or sets the minimum length of the buffer, in bytes, to attempt to fill with each
        /// read from the input stream.
        /// </summary>
        /// <remarks>
        /// If <see cref="BufferPool"/> gives us a larger buffer than what we request, then the
        /// entire buffer will still be used.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when trying to set the value to something that is not greater than zero.
        /// </exception>
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

        /// <summary>
        /// Occurs when each field has been successfully processed.
        /// </summary>
        public event FieldProcessedEventHandler FieldProcessed;

        /// <summary>
        /// Occurs when the last field on a particular line has been processed.
        /// </summary>
        /// <remarks>
        /// Blank lines are ignored, so <see cref="FieldProcessed"/> will always be raised at least
        /// once before this event is raised.
        /// </remarks>
        public event EventHandler EndOfLine;

        /// <summary>
        /// Asynchronously processes a <see cref="Stream"/> assumed to contain UTF-8 encoded CSV
        /// data, following the guidelines listed in RFC 4180.  Data itself and all errors we
        /// encounter are all reported back to the consumer via the various events on this class,
        /// while progress notifications are made available through an (optional) instance of
        /// <see cref="IProgress{T}"/>.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream"/> to read from.
        /// </param>
        /// <param name="progress">
        /// An optional instance of <see cref="IProgress{T}"/> that will receive notifications every
        /// time a chunk of bytes from the input stream has been fully consumed.  The parameter will
        /// be the number of bytes that have been read since the last notification we made, with the
        /// first call containing the number of bytes we read first.
        /// <para>
        /// This parameter may be <see langword="null"/> if no progress notifications are needed.
        /// </para>
        /// <para>
        /// In case it helps, a final notification will be raised, with a parameter value of zero,
        /// when no further events of any kind will be raised and the <see cref="Task"/> is about to
        /// be completed successfully.
        /// </para>
        /// </param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests. The default value is
        /// <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous read operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown (synchronously) when <paramref name="stream"/> is <see langword="null"/>.
        /// </exception>
        public async Task ReadUtf8CsvFileAsync(Stream stream, IProgress<int> progress = null, CancellationToken cancellationToken = default)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

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

            progress?.Report(0);
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
                    // not to look at another byte to see whether or not the double-quote that we
                    // necessarily stopped on here is actually escaping a double-quote that needs to
                    // be written into the cut buffer no matter what.
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
                            // choose to leave it up to the next round to disambiguate.  I call this
                            // a choice, because the buffer size is usually going to be so large
                            // compared to the field lengths, and escaped quotes probably so rare,
                            // that there seems to be a significant speedup potential from looking
                            // ahead that one byte when possible even though we would need something
                            // like what we currently do in order to handle all legal files at all
                            // legal buffer sizes.
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
                        this.FieldProcessed?.Invoke(this, new FieldProcessedEventArgs(fieldBuffer));
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
                this.FieldProcessed?.Invoke(this, new FieldProcessedEventArgs(lastFieldData));

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
