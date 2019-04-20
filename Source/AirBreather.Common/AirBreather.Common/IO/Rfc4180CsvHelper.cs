using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;

namespace AirBreather.IO
{
    public delegate void Rfc4180CsvRowHandler(Rfc4180CsvRow row);

    public static class Rfc4180CsvHelper
    {
        private const byte COMMA = (byte)',';

        private const byte CR = (byte)'\r';

        private const byte LF = (byte)'\n';

        private const byte QUOTE = (byte)'"';

        private static readonly byte[] UnquotedStopBytes = { COMMA, QUOTE, CR, LF };

        public static unsafe void ReadUtf8CsvFile(Stream stream, Rfc4180CsvRowHandler rowHandler)
        {
            stream.ValidateNotNull(nameof(stream));
            rowHandler.ValidateNotNull(nameof(rowHandler));

            var unquotedStopBytes = new ReadOnlySpan<byte>(UnquotedStopBytes);

            byte[] buffer = null;
            var rowOwner = MemoryPool<byte>.Shared.Rent(4096);
            try
            {
                buffer = ArrayPool<byte>.Shared.Rent(4096);

                var fieldOffsets = new List<int>();
                var bufferSpan = new ReadOnlySpan<byte>(buffer);
                var rowSpan = rowOwner.Memory.Span;
                var freeRowSpan = rowSpan;

                bool fieldIsQuoted = false;
                bool escapeNextQuote = false;
                bool ignoreNextLinefeed = false;
                while (true)
                {
                    var currSpan = bufferSpan.Slice(0, stream.Read(buffer, 0, buffer.Length));
                    if (currSpan.IsEmpty)
                    {
                        break;
                    }

                    while (!currSpan.IsEmpty)
                    {
                        int idx;
                        bool stoppedOnEscapedQuote = false;
                        if (fieldIsQuoted && !escapeNextQuote)
                        {
                            idx = currSpan.IndexOf(QUOTE);
                        }
                        else
                        {
                            idx = currSpan.IndexOfAny(unquotedStopBytes);
                            if (escapeNextQuote)
                            {
                                if (idx == 0 && currSpan[0] == QUOTE)
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
                            copyChunk = currSpan;
                            currSpan = default;
                        }
                        else
                        {
                            if (stoppedOnEscapedQuote)
                            {
                                copyChunk = currSpan.Slice(0, 1);
                                currSpan = currSpan.Slice(1);
                            }
                            else
                            {
                                stopByte = currSpan[idx];
                                copyChunk = currSpan.Slice(0, idx);
                                currSpan = currSpan.Slice(idx + 1);
                            }
                        }

                        if (!copyChunk.IsEmpty)
                        {
                            if (copyChunk.Length > freeRowSpan.Length)
                            {
                                int newRowSpanLength = rowSpan.Length, newFreeRowSpanLength = freeRowSpan.Length;
                                do
                                {
                                    newFreeRowSpanLength += newRowSpanLength;
                                    newRowSpanLength *= 2;
                                }
                                while (copyChunk.Length > newFreeRowSpanLength);

                                var rowOwner2 = MemoryPool<byte>.Shared.Rent(newRowSpanLength);
                                try
                                {
                                    var rowSpan2 = rowOwner2.Memory.Span;
                                    var rowSpanConsumed = rowSpan.Slice(0, rowSpan.Length - freeRowSpan.Length);
                                    rowSpanConsumed.CopyTo(rowSpan2.Slice(0, rowSpanConsumed.Length));
                                    rowSpan = rowSpan2;
                                    freeRowSpan = rowSpan.Slice(rowSpanConsumed.Length);
                                    using (var oldRowOwner = rowOwner)
                                    {
                                        rowOwner = rowOwner2;
                                    }
                                }
                                catch
                                {
                                    rowOwner2.Dispose();
                                    throw;
                                }
                            }

                            copyChunk.CopyTo(freeRowSpan.Slice(0, copyChunk.Length));
                            freeRowSpan = freeRowSpan.Slice(copyChunk.Length);
                        }

                        if (ignoreNextLinefeed && stopByte == LF)
                        {
                            stopByte = 0;
                        }

                        ignoreNextLinefeed = false;

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
                                fieldOffsets.Add(rowSpan.Length - freeRowSpan.Length);
                                break;

                            case CR:
                                ignoreNextLinefeed = true;
                                EndLine(rowSpan.Slice(0, rowSpan.Length - freeRowSpan.Length));
                                freeRowSpan = rowSpan;
                                break;

                            case LF:
                                EndLine(rowSpan.Slice(0, rowSpan.Length - freeRowSpan.Length));
                                freeRowSpan = rowSpan;
                                break;
                        }
                    }
                }

                // only call EndLine if the last line was non-empty, since a very common practice is
                // to end text files with an empty blank line.
                if (rowSpan.Length != freeRowSpan.Length)
                {
                    EndLine(rowSpan.Slice(0, rowSpan.Length - freeRowSpan.Length));
                }

                void EndLine(ReadOnlySpan<byte> completeRowSpan)
                {
                    if (fieldOffsets.Count == 0 && completeRowSpan.IsEmpty)
                    {
                        rowHandler(default);
                        return;
                    }

                    using (var slicesOwner = MemoryPool<Rfc4180CsvRow.ColumnSlice>.Shared.Rent(fieldOffsets.Count + 1))
                    {
                        var slicesSpan = slicesOwner.Memory.Span;
                        int i = 0;
                        int prevOffset = 0;
                        foreach (int offset in fieldOffsets)
                        {
                            slicesSpan[i].Offset = prevOffset;
                            slicesSpan[i].Length = offset - prevOffset;
                            prevOffset = offset;
                            ++i;
                        }

                        slicesSpan[i].Offset = prevOffset;
                        slicesSpan[i].Length = completeRowSpan.Length - prevOffset;
                        rowHandler(new Rfc4180CsvRow(completeRowSpan, slicesSpan.Slice(0, i + 1)));
                    }

                    fieldOffsets.Clear();
                }
            }
            finally
            {
                if (buffer != null)
                {
                    ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
                }

                rowOwner?.Dispose();
            }
        }
    }

    public readonly ref struct Rfc4180CsvRow
    {
        private readonly ReadOnlySpan<byte> rawRow;

        private readonly ReadOnlySpan<ColumnSlice> columnSlices;

        internal Rfc4180CsvRow(ReadOnlySpan<byte> rawRow, ReadOnlySpan<ColumnSlice> columnSlices)
        {
            this.rawRow = rawRow;
            this.columnSlices = columnSlices;
        }

        public int FieldCount => this.columnSlices.Length;

        public ReadOnlySpan<byte> this[int index]
        {
            get
            {
                if (unchecked((uint)index >= (uint)columnSlices.Length))
                {
                    throw new ArgumentOutOfRangeException();
                }

                return this.rawRow.Slice(columnSlices[index].Offset, columnSlices[index].Length);
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        public ref struct Enumerator
        {
            private Rfc4180CsvRow row;

            private int curr;

            internal Enumerator(Rfc4180CsvRow row)
            {
                this.row = row;
                this.curr = -1;
            }

            public ReadOnlySpan<byte> Current => this.row.rawRow.Slice(this.row.columnSlices[this.curr].Offset, this.row.columnSlices[this.curr].Length);

            public bool MoveNext() => this.curr < row.columnSlices.Length &&
                                      ++this.curr < row.columnSlices.Length;
        }

        internal struct ColumnSlice
        {
            public int Offset;

            public int Length;
        }
    }
}
