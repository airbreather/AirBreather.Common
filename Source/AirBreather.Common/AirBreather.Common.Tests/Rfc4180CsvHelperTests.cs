using System;
using System.IO;
using System.Text;
using System.Threading;

using AirBreather.IO;

using Xunit;

namespace AirBreather.Tests
{
    public sealed class Rfc4180CsvHelperTests
    {
        [Fact]
        public void TestCsvReading()
        {
            const string CSVText = "Hello,\"Wor\"\"ld\"\r\nhow,\"are,\",you\n\r\n\n\r\n\r\r\n,doing,\"to\"\"\"\"d\"\"ay\",\rI,am,,,,fine\r,,,\nasdf";
            var bytes = Encoding.UTF8.GetBytes(CSVText);
            string[][] expectedRows =
            {
                new[] { "Hello", "Wor\"ld" },
                new[] { "how", "are,", "you" },
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                new[] { string.Empty, "doing", "to\"\"d\"ay", string.Empty },
                new[] { "I", "am", string.Empty, string.Empty, string.Empty, "fine" },
                new[] { string.Empty, string.Empty, string.Empty, string.Empty },
                new[] { "asdf" },
            };
            using (var stream = new MemoryStream(bytes, 0, bytes.Length, writable: false, publiclyVisible: false))
            {
                int i = 0;
                Rfc4180CsvHelper.ReadUtf8CsvFile(stream, row => Do(row, expectedRows[i++]));
            }

            void Do(Rfc4180CsvRow fields, string[] expected)
            {
                string[] actual = new string[fields.FieldCount];
                for (int i = 0; i < actual.Length; i++)
                {
                    actual[i] = Encoding.UTF8.GetString(fields[i]);
                }

                Assert.Equal(expected, actual);

                int j = 0;
                foreach (var field in fields)
                {
                    actual[j++] = Encoding.UTF8.GetString(field);
                }

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void TestDegenerateCsvReading()
        {
            // just one row, 2 fields, field0 is the entire byte array but 1, field1 is empty.
            byte[] bytes = new byte[7654321];
            bytes[bytes.Length - 1] = (byte)',';

            using (var stream = new MemoryStream(bytes, 0, bytes.Length, writable: false, publiclyVisible: false))
            {
                int calledAlready = 0;
                Rfc4180CsvHelper.ReadUtf8CsvFile(stream, row =>
                {
                    Assert.Equal(0, Interlocked.CompareExchange(ref calledAlready, 1, 0));
                    Assert.Equal(2, row.FieldCount);

                    var expected = new ReadOnlySpan<byte>(bytes, 0, bytes.Length - 1);
                    Assert.True(expected.SequenceEqual(row[0]));
                    Assert.True(row[1].IsEmpty);
                });

                Assert.Equal(1, calledAlready);
            }
        }
    }
}
