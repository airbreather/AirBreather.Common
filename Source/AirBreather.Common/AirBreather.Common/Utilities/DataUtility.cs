using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace AirBreather.Common.Utilities
{
    public static class DataUtility
    {
        public static IEnumerable<IDataRecord> ToEnumerable(this IDataReader reader) => ToEnumerableIterator(reader.ValidateNotNull(nameof(reader)));
        private static IEnumerable<IDataRecord> ToEnumerableIterator(IDataReader reader)
        {
            while (reader.Read())
            {
                yield return reader;
            }
        }

        public static IEnumerable<IDataRecord> ToEnumerable(this IDataReader reader, CancellationToken cancellationToken) => ToEnumerableIterator(reader.ValidateNotNull(nameof(reader)), cancellationToken);
        private static IEnumerable<IDataRecord> ToEnumerableIterator(IDataReader reader, CancellationToken cancellationToken)
        {
            // Slightly different than normal here,
            // so that we break on cancellation at
            // the exact right times.
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!reader.Read())
                {
                    yield break;
                }

                yield return reader;
            }
        }
    }
}
