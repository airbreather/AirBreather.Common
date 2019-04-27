using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using AirBreather.Csv;

using CsvHelper;
using CsvHelper.Configuration;

using Xunit;

namespace AirBreather.Tests
{
    public sealed class Rfc4180CsvReaderTests
    {
        private static readonly string TestCsvFilesFolderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(Rfc4180CsvReader)).Location), "TestCsvFiles");

        private static readonly byte[] Utf8BomSequence = { 0xEF, 0xBB, 0xBF };

        public static IEnumerable<object[]> TestCsvFiles =>
            from filePath in Directory.EnumerateFiles(TestCsvFilesFolderPath, "*.csv")
            let fileName = Path.GetFileNameWithoutExtension(filePath)
            from bufferSize in new[] { 1, 2, 3, 5, 8, 13, 21, 34 }
            select new object[] { fileName, bufferSize };

        [Theory]
        [MemberData(nameof(TestCsvFiles))]
        public async Task CompareAirBreatherToCsvHelper(string fileName, int bufferSize)
        {
            fileName = Path.Combine(TestCsvFilesFolderPath, fileName + ".csv");
            var csvHelperReadTask = ReadCsvFileUsingCsvHelperAsync(fileName);
            var airBreatherReadTask = ReadCsvFileUsingAirBreatherAsync(fileName, new Rfc4180CsvReader(), bufferSize);

            var lines = await Task.WhenAll(csvHelperReadTask, airBreatherReadTask).ConfigureAwait(false);
            Assert.Equal(lines[0], lines[1]);
        }

        [Theory]
        [InlineData(32)]
        [InlineData(64)]
        [InlineData(int.MaxValue)]
        public void TestDegenerateCsvReading(int bufferSize)
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

            for (int rem = bytes.Length; rem > 0; rem -= bufferSize)
            {
                helper.ProcessNextReadBufferChunk(new ReadOnlySpan<byte>(bytes, bytes.Length - rem, Math.Min(rem, bufferSize)));
            }

            helper.ProcessNextReadBufferChunk(null);
            gotToEndOfStream = true;

            Assert.Equal(2, fieldProcessedCalls);
            Assert.Equal(1, endOfLineCalls);
        }

        private static async Task<string[][]> ReadCsvFileUsingAirBreatherAsync(string path, Rfc4180CsvReader reader, int fileReadBufferLength)
        {
            var lines = new List<string[]>();
            var currentLine = new List<string>();
            reader.FieldProcessed += OnFieldProcessed;
            reader.EndOfLine += OnEndOfLine;

            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous))
                {
                    byte[] firstBytesBuffer = new byte[Utf8BomSequence.Length];
                    int firstBytes = await stream.LoopedReadAsync(firstBytesBuffer, 0, firstBytesBuffer.Length).ConfigureAwait(false);
                    if (!new ReadOnlySpan<byte>(Utf8BomSequence).SequenceEqual(new ReadOnlySpan<byte>(firstBytesBuffer, 0, firstBytes)))
                    {
                        // it doesn't start with a BOM.
                        // feed what we've read so far into the reader.
                        int offset = 0;
                        while (offset < firstBytes)
                        {
                            int len = Math.Min(firstBytes - offset, fileReadBufferLength);
                            reader.ProcessNextReadBufferChunk(new ReadOnlySpan<byte>(firstBytesBuffer, offset, len));
                            offset += len;
                        }
                    }

                    if (firstBytes < firstBytesBuffer.Length)
                    {
                        // we already processed the entire file.
                        reader.ProcessNextReadBufferChunk(default);
                        return lines.ToArray();
                    }

                    // this is where the fun begins.
                    byte[] mainBuffer = new byte[fileReadBufferLength];
                    int readBytes;
                    while ((readBytes = await stream.ReadAsync(mainBuffer).ConfigureAwait(false)) != 0)
                    {
                        reader.ProcessNextReadBufferChunk(new ReadOnlySpan<byte>(mainBuffer, 0, readBytes));
                    }
                }

                reader.ProcessNextReadBufferChunk(default);
                return lines.ToArray();
            }
            finally
            {
                reader.EndOfLine -= OnEndOfLine;
                reader.FieldProcessed -= OnFieldProcessed;
            }

            void OnFieldProcessed(object sender, FieldProcessedEventArgs args) => currentLine.Add(Encoding.UTF8.GetString(args.Utf8FieldData));
            void OnEndOfLine(object sender, EventArgs args)
            {
                lines.Add(currentLine.ToArray());
                currentLine.Clear();
            }
        }

        private static async Task<string[][]> ReadCsvFileUsingCsvHelperAsync(string path)
        {
            var lines = new List<string[]>();
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous))
            using (var streamReader = new StreamReader(stream, Encoding.UTF8, false))
            using (var csvReader = new CsvReader(streamReader, new Configuration { BadDataFound = null }))
            {
                while (await csvReader.ReadAsync().ConfigureAwait(false))
                {
                    lines.Add(csvReader.Context.Record);
                }
            }

            return lines.ToArray();
        }
    }
}
