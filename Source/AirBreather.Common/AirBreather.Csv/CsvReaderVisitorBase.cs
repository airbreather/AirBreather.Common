using System;

namespace AirBreather.Csv
{
    public abstract class CsvReaderVisitorBase
    {
        public static readonly CsvReaderVisitorBase Null = new NullVisitor();

        public abstract void VisitPartialFieldDataChunk(ReadOnlySpan<byte> chunk);

        public abstract void VisitLastFieldDataChunk(ReadOnlySpan<byte> chunk);

        public abstract void VisitEndOfLine();

        private sealed class NullVisitor : CsvReaderVisitorBase
        {
            public override void VisitEndOfLine() { }

            public override void VisitPartialFieldDataChunk(ReadOnlySpan<byte> chunk) { }

            public override void VisitLastFieldDataChunk(ReadOnlySpan<byte> chunk) { }
        }
    }
}
