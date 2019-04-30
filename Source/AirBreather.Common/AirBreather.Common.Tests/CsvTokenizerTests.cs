using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using AirBreather.BinaryHash;
using AirBreather.Csv;
using AirBreather.Text;

using CsvHelper;
using CsvHelper.Configuration;

using Xunit;

namespace AirBreather.Tests
{
    public sealed class CsvTokenizerTests
    {
        private static readonly string TestCsvFilesFolderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(CsvTokenizer)).Location), "TestCsvFiles");

        public static IEnumerable<object[]> TestCsvFiles =>
            from filePath in Directory.EnumerateFiles(TestCsvFilesFolderPath, "*.csv")
            let fileName = Path.GetFileNameWithoutExtension(filePath)
            from bufferSize in new[] { 1, 2, 3, 5, 8, 13, 21, 34 }
            select new object[] { fileName, bufferSize };

        [Theory]
        [MemberData(nameof(TestCsvFiles))]
        public async Task NullVisitorShouldBeFine(string fileName, int chunkLength)
        {
            // arrange
            string fullCsvFilePath = Path.Combine(TestCsvFilesFolderPath, fileName + ".csv");
            byte[] fileData = await File.ReadAllBytesAsync(fullCsvFilePath).ConfigureAwait(false);
            var tokenizer = new CsvTokenizer();
            int bytesReadSoFar = 0;

            // act
            while (bytesReadSoFar < fileData.Length)
            {
                int thisChunkLength = Math.Min(chunkLength, fileData.Length - bytesReadSoFar);
                tokenizer.ProcessNextChunk(new ReadOnlySpan<byte>(fileData, bytesReadSoFar, thisChunkLength), null);
                bytesReadSoFar += thisChunkLength;
            }

            tokenizer.ProcessEndOfStream(null);

            // assert (empty)
        }

        [Theory]
        [MemberData(nameof(TestCsvFiles))]
        public async Task CsvTokenizationShouldMatchCsvHelper(string fileName, int chunkLength)
        {
            // arrange
            string fullCsvFilePath = Path.Combine(TestCsvFilesFolderPath, fileName + ".csv");
            byte[] fileData = await File.ReadAllBytesAsync(fullCsvFilePath).ConfigureAwait(false);

            // act
            // make sure to test with multiple line-ending variants, including mixed.
            string[][][] allActual = Array.ConvertAll(VaryLineEndings(fileName, chunkLength, fileData),
                                                      csvData => TokenizeCsvFileUsingMine(csvData, new CsvTokenizer(), chunkLength));

            // assert
            string[][][] allExpected = Array.ConvertAll(VaryLineEndings(fileName, chunkLength, fileData),
                                                        csvData => TokenizeCsvFileUsingCsvHelper(csvData));
            for (int i = 0; i < allActual.Length; i++)
            {
                Assert.Equal(allExpected[i], allActual[i]);
            }
        }

        private static string[][] TokenizeCsvFileUsingMine(ReadOnlySpan<byte> fileData, CsvTokenizer tokenizer, int chunkLength)
        {
            var visitor = new StringBufferingVisitor();
            while (fileData.Length >= chunkLength)
            {
                tokenizer.ProcessNextChunk(fileData.Slice(0, chunkLength), visitor);
                fileData = fileData.Slice(chunkLength);
            }

            tokenizer.ProcessNextChunk(fileData, visitor);
            tokenizer.ProcessEndOfStream(visitor);
            return visitor.Finish();
        }

        private static string[][] TokenizeCsvFileUsingCsvHelper(byte[] csvData)
        {
            var lines = new List<string[]>();
            using (var stream = new MemoryStream(csvData, false))
            using (var streamReader = new StreamReader(stream, EncodingEx.UTF8NoBOM, false))
            using (var csvReader = new CsvReader(streamReader, new Configuration { BadDataFound = null }))
            {
                while (csvReader.Read())
                {
                    lines.Add(csvReader.Context.Record);
                }
            }

            return lines.ToArray();
        }

        private static byte[][] VaryLineEndings(string fileName, int chunkLength, ReadOnlySpan<byte> fileData)
        {
            var resultLists = new List<byte>[]
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

            // use a random seed that differs by file but is still consistent across runs.
            ulong fileNameHash = xxHash64.Hash(EncodingEx.UTF8NoBOM.GetBytes(fileName));
            ulong combined = unchecked(fileNameHash + (uint)chunkLength);
            int randomSeed = unchecked((int)combined ^ (int)(combined >> 32));

            var random = new System.Random(randomSeed);

            ReadOnlySpan<byte> newLine = EncodingEx.UTF8NoBOM.GetBytes(Environment.NewLine);
            while (!fileData.IsEmpty)
            {
                int newLineIndex = fileData.IndexOf(newLine);
                byte[] dataBeforeEndOfLine = (newLineIndex < 0 ? fileData : fileData.Slice(0, newLineIndex)).ToArray();

                foreach (var (resultList, i) in resultLists.TagIndexes())
                {
                    resultList.AddRange(dataBeforeEndOfLine);

                    if (i < lineEndings.Length)
                    {
                        // make sure to have, for every line ending, at least one result that uses
                        // that line ending exclusively.
                        resultList.AddRange(lineEndings[i]);
                    }
                    else
                    {
                        // vary the line endings within the rest of the results pseudo-randomly.
                        resultList.AddRange(lineEndings[random.Next(lineEndings.Length)]);
                    }
                }

                fileData = newLineIndex < 0 ? default : fileData.Slice(newLineIndex + newLine.Length);
            }

            return Array.ConvertAll(resultLists, lst => lst.ToArray());
        }

        private sealed class StringBufferingVisitor : CsvReaderVisitorBase
        {
            private readonly List<string[]> _lines = new List<string[]>();

            private readonly List<string> _fields = new List<string>();

            private byte[] _cutBuffer = new byte[4];

            private int _cutBufferConsumed;

            public string[][] Finish()
            {
                var result = _lines.ToArray();
                _lines.Clear();
                return result;
            }

            public override void VisitEndOfRecord()
            {
                _lines.Add(_fields.ToArray());
                _fields.Clear();
            }

            public override void VisitPartialFieldContents(ReadOnlySpan<byte> chunk) => CopyToCutBuffer(chunk);

            public override void VisitEndOfField(ReadOnlySpan<byte> chunk)
            {
                if (_cutBufferConsumed != 0)
                {
                    CopyToCutBuffer(chunk);
                    chunk = new ReadOnlySpan<byte>(_cutBuffer, 0, _cutBufferConsumed);
                }

                _fields.Add(EncodingEx.UTF8NoBOM.GetString(chunk));
                _cutBufferConsumed = 0;
            }

            private void CopyToCutBuffer(ReadOnlySpan<byte> chunk)
            {
                int minLength = _cutBufferConsumed + chunk.Length;
                if (_cutBuffer.Length < minLength)
                {
                    int newLength = _cutBuffer.Length;
                    while (newLength < minLength)
                    {
                        newLength *= 2;
                    }

                    Array.Resize(ref _cutBuffer, newLength);
                }

                chunk.CopyTo(new Span<byte>(_cutBuffer, _cutBufferConsumed, chunk.Length));
                _cutBufferConsumed += chunk.Length;
            }
        }
    }
}
