using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AirBreather.Csv;

using Xunit;

namespace AirBreather.Tests
{
    public sealed class Rfc4180CsvReaderTests
    {
        [Fact]
        public void TestCsvReading()
        {
            const string CSVText = "Héllo,\"Wor\"\"ld\"\r\nhow,\"are,\",you\n\r\n\n\r\n\r\r\n,doing,\"to\"\"\"\"d\"\"a🐨y\",\rI,am,,,,fine\r,,,\n,\nasdf\n";
            var bytes = Encoding.UTF8.GetBytes(CSVText);
            string[][] expectedRows =
            {
                new[] { "Héllo", "Wor\"ld" },
                new[] { "how", "are,", "you" },
                new[] { string.Empty, "doing", "to\"\"d\"a🐨y", string.Empty },
                new[] { "I", "am", string.Empty, string.Empty, string.Empty, "fine" },
                new[] { string.Empty, string.Empty, string.Empty, string.Empty },
                new[] { string.Empty, string.Empty },
                new[] { "asdf" },
            };
            var currentLine = new List<string>();
            int rowsReadSoFar = 0;
            bool gotToEndOfStream = false;
            var helper = new Rfc4180CsvReader { MaxFieldLength = expectedRows.Max(row => row.Length == 0 ? 0 : row.Max(Encoding.UTF8.GetByteCount)) };
            helper.FieldProcessed += (sender, fieldData) =>
            {
                Assert.Equal(helper, sender);

                currentLine.Add(Encoding.UTF8.GetString(fieldData.Utf8FieldData));
            };
            helper.EndOfLine += (sender, args) =>
            {
                Assert.Equal(expectedRows[rowsReadSoFar], currentLine);
                Assert.False(gotToEndOfStream);
                Assert.Equal(helper, sender);

                ++rowsReadSoFar;
                currentLine.Clear();
            };

            helper.ProcessNextReadBufferChunk(bytes);
            helper.ProcessNextReadBufferChunk(null);
            gotToEndOfStream = true;

            Assert.Equal(expectedRows.Length, rowsReadSoFar);
        }

        [Fact]
        public void TestDegenerateCsvReading()
        {
            // just one row, 2 fields, field0 is the entire byte array but 1, field1 is empty.
            byte[] bytes = new byte[7654321];
            bytes[bytes.Length - 1] = (byte)',';

            var helper = new Rfc4180CsvReader { MaxFieldLength = bytes.Length - 1 };
            int fieldProcessedCalls = 0;
            int endOfLineCalls = 0;
            bool gotToEndOfStream = false;
            helper.FieldProcessed += (sender, actual) =>
            {
                switch (++fieldProcessedCalls)
                {
                    case 1:
                        var expected = new ReadOnlySpan<byte>(bytes, 0, bytes.Length - 1);
                        Assert.True(expected.SequenceEqual(actual.Utf8FieldData));
                        break;

                    case 2:
                        Assert.True(actual.Utf8FieldData.IsEmpty);
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

            helper.ProcessNextReadBufferChunk(bytes);
            helper.ProcessNextReadBufferChunk(null);
            gotToEndOfStream = true;

            Assert.Equal(2, fieldProcessedCalls);
            Assert.Equal(1, endOfLineCalls);
        }
    }
}
