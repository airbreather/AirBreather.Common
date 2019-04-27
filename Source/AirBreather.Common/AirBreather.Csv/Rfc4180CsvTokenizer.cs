using System;
using System.IO;
using System.Runtime.CompilerServices;

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
    public class Rfc4180CsvTokenizer
    {
        private const byte COMMA = (byte)',';

        private const byte CR = (byte)'\r';

        private const byte LF = (byte)'\n';

        private const byte QUOTE = (byte)'"';

        private static readonly byte[] AllStopBytes = { COMMA, QUOTE, CR, LF };

        private byte[] _cutFieldBuffer = new byte[81920];

        private int _cutFieldBufferConsumed;

        private ParserFlags _parserFlags;

        [Flags]
        private enum ParserFlags : byte
        {
            None,
            ReadAnythingOnCurrentLine           = 0b00000001,
            ReadAnythingInCurrentField          = 0b00000010,
            CurrentFieldStartedWithQuote        = 0b00000100,
            QuotedFieldDataEnded                = 0b00001000,
            CutAtPotentiallyTerminalDoubleQuote = 0b00010000,
            FieldDataSoFarExceedsMaxLength      = 0b00100000,
        }

        public int MaxFieldLength
        {
            get => _cutFieldBuffer.Length;
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Must be greater than zero.");
                }

                _cutFieldBuffer = new byte[value];
            }
        }

        public void ProcessNextReadBufferChunk(ReadOnlySpan<byte> readBuffer, CsvReaderVisitorBase visitor)
        {
            if (visitor is null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            ReadOnlySpan<byte> allStopBytes = AllStopBytes;

            // we're going to consume the entire buffer that was handed to us.
            while (!readBuffer.IsEmpty)
            {
                if ((_parserFlags & ParserFlags.ReadAnythingInCurrentField) != 0)
                {
                    // most of the time, we should be able to fully process each field in the same
                    // loop iteration that we first start reading it.  the most prominent exception
                    // is that 
                    PickUpFromLastTime(ref readBuffer, visitor);
                    continue;
                }

                int idx = readBuffer.IndexOfAny(allStopBytes);
                if (idx < 0)
                {
                    CopyToCutBuffer(readBuffer, visitor);
                    _parserFlags = ParserFlags.ReadAnythingInCurrentField | ParserFlags.ReadAnythingOnCurrentLine;
                    break;
                }

                switch (readBuffer[idx])
                {
                    case QUOTE:
                        if (idx == 0)
                        {
                            _parserFlags = ParserFlags.CurrentFieldStartedWithQuote | ParserFlags.ReadAnythingInCurrentField | ParserFlags.ReadAnythingOnCurrentLine;
                        }
                        else
                        {
                            // RFC 4180 forbids quotes that show up anywhere but the beginning of a
                            // field, so it's up to us to decide what we want to do about this.  We
                            // choose to treat all such quotes as just regular data.
                            CopyToCutBuffer(readBuffer.Slice(0, idx + 1), visitor);
                            _parserFlags = ParserFlags.ReadAnythingInCurrentField | ParserFlags.ReadAnythingOnCurrentLine;
                        }

                        break;

                    case COMMA:
                        ProcessEndOfField(readBuffer.Slice(0, idx), visitor);
                        break;

                    default:
                        ProcessEndOfLine(readBuffer.Slice(0, idx), visitor);
                        break;
                }

                readBuffer = readBuffer.Slice(idx + 1);
            }
        }

        public void ProcessFinalReadBufferChunk(CsvReaderVisitorBase visitor)
        {
            if (visitor is null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            ProcessEndOfLine(default, visitor);
        }

        private void PickUpFromLastTime(ref ReadOnlySpan<byte> readBuffer, CsvReaderVisitorBase visitor)
        {
            if ((_parserFlags & ParserFlags.CutAtPotentiallyTerminalDoubleQuote) != 0)
            {
                HandleBufferCutAtPotentiallyTerminalDoubleQuote(ref readBuffer, visitor);
                return;
            }

            if ((_parserFlags & (ParserFlags.CurrentFieldStartedWithQuote | ParserFlags.QuotedFieldDataEnded)) == ParserFlags.CurrentFieldStartedWithQuote)
            {
                int idx = readBuffer.IndexOf(QUOTE);
                if (idx < 0)
                {
                    CopyToCutBuffer(readBuffer, visitor);
                    readBuffer = default;
                    return;
                }

                if (idx == readBuffer.Length - 1)
                {
                    CopyToCutBuffer(readBuffer.Slice(0, idx), visitor);
                    _parserFlags |= ParserFlags.CutAtPotentiallyTerminalDoubleQuote;
                    readBuffer = default;
                    return;
                }

                switch (readBuffer[idx + 1])
                {
                    case QUOTE:
                        // escaped quote, copy it in and send it back
                        CopyToCutBuffer(readBuffer.Slice(0, idx + 1), visitor);
                        break;

                    case COMMA:
                        ProcessEndOfField(readBuffer.Slice(0, idx), visitor);
                        break;

                    case CR:
                    case LF:
                        ProcessEndOfLine(readBuffer.Slice(0, idx), visitor);
                        break;

                    default:
                        _parserFlags |= ParserFlags.QuotedFieldDataEnded;
                        CopyToCutBuffer(readBuffer.Slice(0, idx), visitor);
                        CopyToCutBuffer(readBuffer.Slice(idx + 1, 1), visitor);
                        break;
                }

                readBuffer = readBuffer.Slice(idx + 2);
                return;
            }

            // this is expected to be rare: either we were cut between field reads, or we're reading
            // nonstandard field data where the quoted field data ends but there's extra stuff after
            {
                ReadOnlySpan<byte> allStopBytes = stackalloc byte[] { COMMA, CR, LF };
                int idx = readBuffer.IndexOfAny(allStopBytes);
                if (idx < 0)
                {
                    CopyToCutBuffer(readBuffer, visitor);
                    readBuffer = default;
                    return;
                }

                switch (readBuffer[idx])
                {
                    case COMMA:
                        ProcessEndOfField(readBuffer.Slice(0, idx), visitor);
                        break;

                    default:
                        ProcessEndOfLine(readBuffer.Slice(0, idx), visitor);
                        break;
                }

                readBuffer = readBuffer.Slice(idx + 1);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void HandleBufferCutAtPotentiallyTerminalDoubleQuote(ref ReadOnlySpan<byte> readBuffer, CsvReaderVisitorBase visitor)
        {
            // this method is only called in the rare case where the very last character of the last
            // read buffer was a stopping double quote while we were reading quoted field data, so
            // this method is expected to be called so rarely in performance-sensitive cases that I
            // don't think it will ever pay off to bother doing more processing here.  so we just do
            // the minimum amount that we need to do in order to clear this flag and get back into
            // the normal swing of things.
            _parserFlags &= ~ParserFlags.CutAtPotentiallyTerminalDoubleQuote;
            switch (readBuffer[0])
            {
                case QUOTE:
                    // the previous double quote was actually there to escape this double quote.  we
                    // didn't copy the double quote into our cut buffer last time because we weren't
                    // sure.  well, we're sure now, so go ahead copy it.
                    CopyToCutBuffer(readBuffer.Slice(0, 1), visitor);

                    // we processed the double quote, so main loop should resume at the next byte.
                    readBuffer = readBuffer.Slice(1);
                    break;

                default:
                    // the previous double quote did in fact terminate the quoted part of the field
                    // data, and so all we need to do is set this flag..  main loop will re-process
                    // this buffer and go about its merry way.
                    _parserFlags |= ParserFlags.QuotedFieldDataEnded;
                    break;
            }
        }

        private ReadOnlySpan<byte> CopyToCutBuffer(ReadOnlySpan<byte> copyBuffer, CsvReaderVisitorBase visitor)
        {
            if ((_parserFlags & ParserFlags.FieldDataSoFarExceedsMaxLength) == 0)
            {
                if (_cutFieldBufferConsumed + copyBuffer.Length <= _cutFieldBuffer.Length)
                {
                    copyBuffer.CopyTo(new Span<byte>(_cutFieldBuffer, _cutFieldBufferConsumed, copyBuffer.Length));
                    _cutFieldBufferConsumed += copyBuffer.Length;
                    return new ReadOnlySpan<byte>(_cutFieldBuffer, 0, _cutFieldBufferConsumed);
                }
                else
                {
                    int bytesToCopy = _cutFieldBuffer.Length - _cutFieldBufferConsumed;
                    copyBuffer.Slice(0, bytesToCopy).CopyTo(new Span<byte>(_cutFieldBuffer, _cutFieldBufferConsumed, bytesToCopy));
                    _cutFieldBufferConsumed = _cutFieldBuffer.Length;
                    visitor.VisitStartOfOverflowingFieldData(_cutFieldBuffer);
                    _parserFlags |= ParserFlags.FieldDataSoFarExceedsMaxLength;
                }
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessEndOfField(ReadOnlySpan<byte> lastReadSection, CsvReaderVisitorBase visitor)
        {
            visitor.VisitFieldData(_cutFieldBufferConsumed == 0
                ? lastReadSection
                : CopyToCutBuffer(lastReadSection, visitor));
            _parserFlags = ParserFlags.ReadAnythingOnCurrentLine;
            _cutFieldBufferConsumed = 0;
        }

        private void ProcessEndOfLine(ReadOnlySpan<byte> lastFieldData, CsvReaderVisitorBase visitor)
        {
            if (!lastFieldData.IsEmpty || (_parserFlags & ParserFlags.ReadAnythingOnCurrentLine) != 0)
            {
                ProcessEndOfField(lastFieldData, visitor);
                visitor.VisitEndOfLine();
            }

            _parserFlags = ParserFlags.None;
        }
    }
}
