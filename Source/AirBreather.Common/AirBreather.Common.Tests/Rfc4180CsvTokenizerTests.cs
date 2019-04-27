using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using AirBreather.Csv;
using AirBreather.Text;

using CsvHelper;
using CsvHelper.Configuration;

using Xunit;

namespace AirBreather.Tests
{
    public sealed class Rfc4180CsvTokenizerTests
    {
        private static readonly string TestCsvFilesFolderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(Rfc4180CsvTokenizer)).Location), "TestCsvFiles");

        public static IEnumerable<object[]> TestCsvFiles =>
            from filePath in Directory.EnumerateFiles(TestCsvFilesFolderPath, "*.csv")
            let fileName = Path.GetFileNameWithoutExtension(filePath)
            from bufferSize in new[] { 1, 2, 3, 5, 8, 13, 21, 34 }
            select new object[] { fileName, bufferSize };

        [Theory]
        [MemberData(nameof(TestCsvFiles))]
        public async Task CompareToCsvHelper(string fileName, int bufferSize)
        {
            string fullCsvFilePath = Path.Combine(TestCsvFilesFolderPath, fileName + ".csv");
            var csvHelperReadTask = ReadCsvFileUsingCsvHelperAsync(fullCsvFilePath);
            var airBreatherReadTask = ReadCsvFileUsingMineAsync(fullCsvFilePath, new Rfc4180CsvTokenizer(), bufferSize);

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

            var tokenizer = new Rfc4180CsvTokenizer { MaxFieldLength = bytes.Length - 1 };
            int fieldProcessedCalls = 0;
            int endOfLineCalls = 0;
            bool gotToEndOfStream = false;

            var visitor = new DelegateVisitor(VisitFieldData, VisitEndOfLine);
            void VisitFieldData(ReadOnlySpan<byte> fieldData)
            {
                switch (++fieldProcessedCalls)
                {
                    case 1:
                        var expected = new ReadOnlySpan<byte>(bytes, 0, bytes.Length - 1);
                        Assert.True(expected.SequenceEqual(fieldData));
                        break;

                    case 2:
                        Assert.True(fieldData.IsEmpty);
                        break;

                    default:
                        Assert.True(false, "Expected 2 fields");
                        break;
                }

                Assert.Equal(0, endOfLineCalls);
                Assert.False(gotToEndOfStream);
            }
            void VisitEndOfLine()
            {
                Assert.Equal(1, ++endOfLineCalls);
                Assert.Equal(2, fieldProcessedCalls);
                Assert.False(gotToEndOfStream);
            }

            for (int rem = bytes.Length; rem > 0; rem -= bufferSize)
            {
                tokenizer.ProcessNextReadBufferChunk(new ReadOnlySpan<byte>(bytes, bytes.Length - rem, Math.Min(rem, bufferSize)), visitor);
            }

            tokenizer.ProcessFinalReadBufferChunk(visitor);
            gotToEndOfStream = true;

            Assert.Equal(2, fieldProcessedCalls);
            Assert.Equal(1, endOfLineCalls);
        }

        private static async Task<string[][]> ReadCsvFileUsingMineAsync(string path, Rfc4180CsvTokenizer tokenizer, int fileReadBufferLength)
        {
            var visitor = new StringBufferingVisitor();
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous))
            {
                byte[] buffer = new byte[fileReadBufferLength];
                int readBytes;
                while ((readBytes = await stream.ReadAsync(buffer).ConfigureAwait(false)) != 0)
                {
                    tokenizer.ProcessNextReadBufferChunk(new ReadOnlySpan<byte>(buffer, 0, readBytes), visitor);
                }
            }

            tokenizer.ProcessFinalReadBufferChunk(visitor);
            return visitor.Finish();
        }

        private static async Task<string[][]> ReadCsvFileUsingCsvHelperAsync(string path)
        {
            var lines = new List<string[]>();
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous))
            using (var streamReader = new StreamReader(stream, EncodingEx.UTF8NoBOM, false))
            using (var csvReader = new CsvReader(streamReader, new Configuration { BadDataFound = null }))
            {
                while (await csvReader.ReadAsync().ConfigureAwait(false))
                {
                    lines.Add(csvReader.Context.Record);
                }
            }

            return lines.ToArray();
        }

        private sealed class StringBufferingVisitor : CsvReaderVisitorBase
        {
            private static readonly UTF8Encoding encoding = new UTF8Encoding(false, false);

            private readonly List<string[]> _lines = new List<string[]>();

            private readonly List<string> _fields = new List<string>();

            public string[][] Finish()
            {
                string[][] result = _lines.ToArray();
                _lines.Clear();
                return result;
            }

            public override void VisitEndOfLine()
            {
                _lines.Add(_fields.ToArray());
                _fields.Clear();
            }

            public override void VisitFieldData(ReadOnlySpan<byte> fieldData) => _fields.Add(encoding.GetString(fieldData));
        }

        private sealed class DelegateVisitor : CsvReaderVisitorBase
        {
            private readonly VisitFieldDataDelegate _visitFieldData;

            private readonly Action _visitEndOfLine;

            public delegate void VisitFieldDataDelegate(ReadOnlySpan<byte> fieldData);

            public DelegateVisitor(VisitFieldDataDelegate visitFieldData, Action visitEndOfLine) =>
                (_visitFieldData, _visitEndOfLine) = (visitFieldData, visitEndOfLine);

            public override void VisitEndOfLine() => _visitEndOfLine();

            public override void VisitFieldData(ReadOnlySpan<byte> fieldData) => _visitFieldData(fieldData);
        }
    }
}
