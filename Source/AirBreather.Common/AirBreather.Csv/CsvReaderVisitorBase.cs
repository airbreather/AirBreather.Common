using System;

namespace AirBreather.Csv
{
    public abstract class CsvReaderVisitorBase
    {
        public abstract void VisitFieldData(ReadOnlySpan<byte> fieldData);

        public abstract void VisitEndOfLine();
    }
}
