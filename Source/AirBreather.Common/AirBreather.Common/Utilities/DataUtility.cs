using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace AirBreather.Common.Utilities
{
    public static class DataUtility
    {
        public static IEnumerable<IDataReader> ToEnumerable(this IDataReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            while (reader.Read())
            {
                yield return reader;
            }
        }

        public static IEnumerable<IDataReader> ToEnumerable(this IDataReader reader, CancellationToken cancellationToken)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

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
