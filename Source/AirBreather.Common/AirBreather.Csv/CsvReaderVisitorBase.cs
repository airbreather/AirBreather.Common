using System;
using System.IO;

using static System.FormattableString;

namespace AirBreather.Csv
{
    public abstract class CsvReaderVisitorBase
    {
        public static readonly CsvReaderVisitorBase Null = new NullVisitor();

        public abstract void VisitFieldData(ReadOnlySpan<byte> fieldData);

        public abstract void VisitEndOfLine();

        public virtual void VisitStartOfOverflowingFieldData(ReadOnlySpan<byte> bytesRead)
        {
            throw new InvalidDataException(Invariant($"Data contains one or more fields that exceed the limit of {bytesRead.Length} set in {nameof(Rfc4180CsvTokenizer.MaxFieldLength)}."));
        }

        private sealed class NullVisitor : CsvReaderVisitorBase
        {
            public override void VisitEndOfLine() { }

            public override void VisitFieldData(ReadOnlySpan<byte> fieldData) { }

            public override void VisitStartOfOverflowingFieldData(ReadOnlySpan<byte> bytesRead) { }
        }
    }
}
