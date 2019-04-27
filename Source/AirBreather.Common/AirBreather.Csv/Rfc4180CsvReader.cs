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
    public class Rfc4180CsvReader
    {
        private const byte COMMA = (byte)',';

        private const byte CR = (byte)'\r';

        private const byte LF = (byte)'\n';

        private const byte QUOTE = (byte)'"';

        private byte[] cutFieldBuffer;

        private int cutFieldBufferConsumed;

        private ParserFlags parserFlags;

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

        public int MaxFieldLength
        {
            get => CutFieldBuffer.Length;
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Must be greater than zero.");
                }

                cutFieldBuffer = new byte[value];
            }
        }

        private byte[] CutFieldBuffer => cutFieldBuffer ?? (cutFieldBuffer = new byte[81920]);

        public void ProcessNextReadBufferChunk(ReadOnlySpan<byte> readBuffer)
        {
            if (readBuffer.IsEmpty)
            {
                ProcessEndOfLine(readBuffer);
                return;
            }

            // we're going to consume the entire buffer that was handed to us.
            ReadOnlySpan<byte> allStopBytes = stackalloc byte[] { COMMA, QUOTE, CR, LF };

            do
            {
                if ((parserFlags & ParserFlags.ReadAnythingInCurrentField) != 0)
                {
                    PickUpFromLastTime(ref readBuffer);
                    continue;
                }

                int idx = readBuffer.IndexOfAny(allStopBytes);
                if (idx < 0)
                {
                    CopyToCutBuffer(readBuffer);
                    parserFlags = ParserFlags.ReadAnythingInCurrentField | ParserFlags.ReadAnythingOnCurrentLine;
                    break;
                }

                switch (readBuffer[idx])
                {
                    case QUOTE:
                        if (idx == 0)
                        {
                            parserFlags = ParserFlags.CurrentFieldStartedWithQuote | ParserFlags.ReadAnythingInCurrentField | ParserFlags.ReadAnythingOnCurrentLine;
                        }
                        else
                        {
                            CopyToCutBuffer(readBuffer.Slice(0, idx + 1));
                            parserFlags = ParserFlags.ReadAnythingInCurrentField | ParserFlags.ReadAnythingOnCurrentLine;
                        }

                        break;

                    case COMMA:
                        ProcessEndOfField(readBuffer.Slice(0, idx));
                        break;

                    default:
                        ProcessEndOfLine(readBuffer.Slice(0, idx));
                        break;
                }

                readBuffer = readBuffer.Slice(idx + 1);
            }
            while (!readBuffer.IsEmpty);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void PickUpFromLastTime(ref ReadOnlySpan<byte> readBuffer)
        {
            if ((parserFlags & ParserFlags.CutAtPotentiallyTerminalDoubleQuote) != 0)
            {
                HandleBufferCutAtPotentiallyTerminalDoubleQuote(ref readBuffer);
                return;
            }

            if ((parserFlags & (ParserFlags.CurrentFieldStartedWithQuote | ParserFlags.QuotedFieldDataEnded)) == ParserFlags.CurrentFieldStartedWithQuote)
            {
                int idx = readBuffer.IndexOf(QUOTE);
                if (idx < 0)
                {
                    CopyToCutBuffer(readBuffer);
                    readBuffer = default;
                    return;
                }

                if (idx == readBuffer.Length - 1)
                {
                    CopyToCutBuffer(readBuffer.Slice(0, idx));
                    parserFlags |= ParserFlags.CutAtPotentiallyTerminalDoubleQuote;
                    readBuffer = default;
                    return;
                }

                switch (readBuffer[idx + 1])
                {
                    case QUOTE:
                        // escaped quote, copy it in and send it back
                        CopyToCutBuffer(readBuffer.Slice(0, idx + 1));
                        break;

                    case COMMA:
                        ProcessEndOfField(readBuffer.Slice(0, idx));
                        break;

                    case CR:
                    case LF:
                        ProcessEndOfLine(readBuffer.Slice(0, idx));
                        break;

                    default:
                        parserFlags |= ParserFlags.QuotedFieldDataEnded;
                        CopyToCutBuffer(readBuffer.Slice(0, idx));
                        CopyToCutBuffer(readBuffer.Slice(idx + 1, 1));
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
                    CopyToCutBuffer(readBuffer);
                    readBuffer = default;
                    return;
                }

                switch (readBuffer[idx])
                {
                    case COMMA:
                        ProcessEndOfField(readBuffer.Slice(0, idx));
                        break;

                    default:
                        ProcessEndOfLine(readBuffer.Slice(0, idx));
                        break;
                }

                readBuffer = readBuffer.Slice(idx + 1);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void HandleBufferCutAtPotentiallyTerminalDoubleQuote(ref ReadOnlySpan<byte> readBuffer)
        {
            // like I mentioned in the comments near the caller, this method's job is just to do the
            // special processing we need to do in order to clear the flag and move on.  this method
            // is expected to be called so rarely, at least in performance-sensitive cases, that I
            // don't think it will ever pay off to bother doing more processing here.
            parserFlags &= ~ParserFlags.CutAtPotentiallyTerminalDoubleQuote;
            switch (readBuffer[0])
            {
                case QUOTE:
                    CopyToCutBuffer(readBuffer.Slice(0, 1));
                    readBuffer = readBuffer.Slice(1);
                    break;

                default:
                    parserFlags |= ParserFlags.QuotedFieldDataEnded;
                    break;
            }
        }

        private ReadOnlySpan<byte> CopyToCutBuffer(ReadOnlySpan<byte> copyBuffer)
        {
            if ((parserFlags & ParserFlags.FieldDataSoFarExceedsMaxLength) == 0)
            {
                if (cutFieldBufferConsumed + copyBuffer.Length <= CutFieldBuffer.Length)
                {
                    copyBuffer.CopyTo(new Span<byte>(cutFieldBuffer, cutFieldBufferConsumed, copyBuffer.Length));
                    cutFieldBufferConsumed += copyBuffer.Length;
                    return new ReadOnlySpan<byte>(cutFieldBuffer, 0, cutFieldBufferConsumed);
                }
                else
                {
                    ThrowFieldTooLongError();
                    parserFlags |= ParserFlags.FieldDataSoFarExceedsMaxLength;
                }
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessEndOfField(ReadOnlySpan<byte> lastReadSection)
        {
            ReadOnlySpan<byte> fieldBuffer = cutFieldBufferConsumed == 0
                ? lastReadSection
                : CopyToCutBuffer(lastReadSection);

            FieldProcessed?.Invoke(this, new FieldProcessedEventArgs(fieldBuffer));
            parserFlags = ParserFlags.ReadAnythingOnCurrentLine;
            cutFieldBufferConsumed = 0;
        }

        private void ProcessEndOfLine(ReadOnlySpan<byte> lastFieldData)
        {
            if (!lastFieldData.IsEmpty || (parserFlags & ParserFlags.ReadAnythingOnCurrentLine) != 0)
            {
                ProcessEndOfField(lastFieldData);
                EndOfLine?.Invoke(this, EventArgs.Empty);
            }

            parserFlags = ParserFlags.None;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowFieldTooLongError() => throw new InvalidDataException($"Data contains one or more fields that exceed the limit set in {nameof(MaxFieldLength)}.");
    }
}
