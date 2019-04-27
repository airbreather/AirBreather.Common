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
    }
}
