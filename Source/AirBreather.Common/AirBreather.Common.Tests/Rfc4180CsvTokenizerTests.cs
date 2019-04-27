using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            var csvData = VaryLineEndings(await File.ReadAllBytesAsync(fullCsvFilePath).ConfigureAwait(false));
            var airBreatherReadTasks = Array.ConvertAll(csvData,
                                                        innerCsvData => ReadCsvFileUsingMineAsync(new MemoryStream(innerCsvData, false),
                                                                                                  new Rfc4180CsvTokenizer(),
                                                                                                  bufferSize));

            var csvHelperReadTask = ReadCsvFileUsingCsvHelperAsync(fullCsvFilePath);
            string[][] expected = await csvHelperReadTask.ConfigureAwait(false);
            Assert.All(await Task.WhenAll(airBreatherReadTasks).ConfigureAwait(false), actual => Assert.Equal(expected, actual));
        }

        private static async Task<string[][]> ReadCsvFileUsingMineAsync(Stream stream, Rfc4180CsvTokenizer tokenizer, int fileReadBufferLength)
        {
            var visitor = new StringBufferingVisitor();
            byte[] buffer = new byte[fileReadBufferLength];
            int readBytes;
            while ((readBytes = await stream.ReadAsync(buffer).ConfigureAwait(false)) != 0)
            {
                tokenizer.ProcessNextReadBufferChunk(new ReadOnlySpan<byte>(buffer, 0, readBytes), visitor);
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

        private static byte[][] VaryLineEndings(ReadOnlySpan<byte> original)
        {
            var resultList = new List<byte>[]
            {
                new List<byte>(),
                new List<byte>(),
                new List<byte>(),
                new List<byte>(),
                new List<byte>(),
                new List<byte>(),
            };

            var lineEndings = new byte[][]
            {
                new byte[] { (byte)'\r' },
                new byte[] { (byte)'\n' },
                new byte[] { (byte)'\r', (byte)'\n' },
            };

            var random = new System.Random(8675309);
            ReadOnlySpan<byte> newLine = EncodingEx.UTF8NoBOM.GetBytes(Environment.NewLine);
            while (true)
            {
                int idx = original.IndexOf(newLine);
                byte[] final = (idx < 0 ? original : original.Slice(0, idx)).ToArray();
                foreach (var list in resultList)
                {
                    list.AddRange(final);
                }

                if (idx < 0)
                {
                    break;
                }

                // first three use the Mac, Unix, and Windows line endings
                resultList[0].AddRange(lineEndings[0]);
                resultList[1].AddRange(lineEndings[1]);
                resultList[2].AddRange(lineEndings[2]);

                // last three vary line endings within the file, pseudo-randomly.
                resultList[3].AddRange(lineEndings[random.Next(3)]);
                resultList[4].AddRange(lineEndings[random.Next(3)]);
                resultList[5].AddRange(lineEndings[random.Next(3)]);

                original = original.Slice(idx + newLine.Length);
            }

            return Array.ConvertAll(resultList, lst => lst.ToArray());
        }

        private sealed class StringBufferingVisitor : CsvReaderVisitorBase
        {
            private readonly List<string[]> _lines = new List<string[]>();

            private readonly List<string> _fields = new List<string>();

            public string[][] Finish()
            {
                var result = _lines.ToArray();
                _lines.Clear();
                return result;
            }

            public override void VisitEndOfLine()
            {
                _lines.Add(_fields.ToArray());
                _fields.Clear();
            }

            public override void VisitFieldData(ReadOnlySpan<byte> fieldData) => _fields.Add(EncodingEx.UTF8NoBOM.GetString(fieldData));
        }
    }
}
