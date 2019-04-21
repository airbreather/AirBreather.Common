using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AirBreather.IO;

using Xunit;

namespace AirBreather.Tests
{
    public sealed class Rfc4180CsvHelperTests
    {
        [Fact]
        public async Task TestCsvReading()
        {
            const string CSVText = "Hello,\"Wor\"\"ld\"\r\nhow,\"are,\",you\n\r\n\n\r\n\r\r\n,doing,\"to\"\"\"\"d\"\"ay\",\rI,am,,,,fine\r,,,\nasdf\n";
            var bytes = Encoding.UTF8.GetBytes(CSVText);
            string[][] expectedRows =
            {
                new[] { "Hello", "Wor\"ld" },
                new[] { "how", "are,", "you" },
                new[] { string.Empty, "doing", "to\"\"d\"ay", string.Empty },
                new[] { "I", "am", string.Empty, string.Empty, string.Empty, "fine" },
                new[] { string.Empty, string.Empty, string.Empty, string.Empty },
                new[] { "asdf" },
            };
            using (var stream = new MemoryStream(bytes, 0, bytes.Length, writable: false, publiclyVisible: false))
            {
                var currentLine = new List<string>();
                int rowsReadSoFar = 0;
                bool gotToEndOfStream = false;
                var helper = new Rfc4180CsvHelper { MaxFieldLength = expectedRows.Max(row => row.Length == 0 ? 0 : row.Max(Encoding.UTF8.GetByteCount)) };
                helper.FieldProcessed += (sender, fieldData) =>
                {
                    Assert.Equal(helper, sender);

                    currentLine.Add(Encoding.UTF8.GetString(fieldData));
                };
                helper.EndOfLine += (sender, args) =>
                {
                    Assert.Equal(expectedRows[rowsReadSoFar], currentLine);
                    Assert.False(gotToEndOfStream);
                    Assert.Equal(helper, sender);

                    ++rowsReadSoFar;
                    currentLine.Clear();
                };

                await helper.ReadUtf8CsvFileAsync(stream);
                gotToEndOfStream = true;

                Assert.Equal(expectedRows.Length, rowsReadSoFar);
            }
        }

        [Fact]
        public async Task TestDegenerateCsvReading()
        {
            // just one row, 2 fields, field0 is the entire byte array but 1, field1 is empty.
            byte[] bytes = new byte[7654321];
            bytes[bytes.Length - 1] = (byte)',';

            using (var stream = new MemoryStream(bytes, 0, bytes.Length, writable: false, publiclyVisible: false))
            {
                var helper = new Rfc4180CsvHelper { MaxFieldLength = bytes.Length - 1 };
                int fieldProcessedCalls = 0;
                int endOfLineCalls = 0;
                bool gotToEndOfStream = false;
                helper.FieldProcessed += (sender, actual) =>
                {
                    switch (++fieldProcessedCalls)
                    {
                        case 1:
                            var expected = new ReadOnlySpan<byte>(bytes, 0, bytes.Length - 1);
                            Assert.True(expected.SequenceEqual(actual));
                            break;

                        case 2:
                            Assert.True(actual.IsEmpty);
                            break;

                        default:
                            Assert.True(false, "Expected 2 fields");
                            break;
                    }

                    Assert.Equal(0, endOfLineCalls);
                    Assert.Equal(helper, sender);
                    Assert.False(gotToEndOfStream);
                };

                helper.EndOfLine += (sender, args) =>
                {
                    Assert.Equal(1, ++endOfLineCalls);
                    Assert.Equal(2, fieldProcessedCalls);
                    Assert.Equal(helper, sender);
                    Assert.False(gotToEndOfStream);
                };

                await helper.ReadUtf8CsvFileAsync(stream).ConfigureAwait(true);
                gotToEndOfStream = true;

                Assert.Equal(2, fieldProcessedCalls);
                Assert.Equal(1, endOfLineCalls);
            }
        }
    }
}
