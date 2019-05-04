using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using AirBreather.Csv;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using CsvHelper;
using CsvHelper.Configuration;

namespace AirBreather.Bench
{
    [ClrJob]
    [CoreJob]
    [CoreRtJob]
    [GcServer(true)]
    [MemoryDiagnoser]
    public class Program
    {
        public static CsvFile[] CsvFiles => GetCsvFiles();

        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(CsvFiles))]
        public long CountRowsUsingMine(CsvFile csvFile)
        {
            var visitor = new RowCountingVisitor();
            var tokenizer = new CsvTokenizer();
            tokenizer.ProcessNextChunk(csvFile.FileData, visitor);
            tokenizer.ProcessEndOfStream(visitor);
            return visitor.RowCount;
        }

        [Benchmark]
        [ArgumentsSource(nameof(CsvFiles))]
        public long CountRowsUsingCsvHelper(CsvFile csvFile)
        {
            using (var ms = new MemoryStream(csvFile.FileData, false))
            using (var tr = new StreamReader(ms, new UTF8Encoding(false, true), false))
            using (var rd = new CsvReader(tr, new Configuration { BadDataFound = null }))
            {
                long cnt = 0;
                while (rd.Read())
                {
                    ++cnt;
                }

                return cnt;
            }
        }

        static int Main()
        {
            var prog = new Program();
            foreach (var csvFile in CsvFiles)
            {
                if (prog.CountRowsUsingMine(csvFile) != prog.CountRowsUsingCsvHelper(csvFile))
                {
                    Console.Error.WriteLine($"Failed on {csvFile}.");
                    return 1;
                }
            }

            BenchmarkRunner.Run<Program>();
            return 0;
        }

        public readonly struct CsvFile
        {
            public CsvFile(string fullPath) =>
                (FullPath, FileName, FileData) = (fullPath, Path.GetFileNameWithoutExtension(fullPath), File.ReadAllBytes(fullPath));

            public string FullPath { get; }

            public string FileName { get; }

            public byte[] FileData { get; }

            public override string ToString() => FileName;
        }

        static CsvFile[] GetCsvFiles([CallerFilePath]string myLocation = null)
        {
            return Array.ConvertAll(Directory.GetFiles(Path.Combine(Path.GetDirectoryName(myLocation), "large-data-files"), "*.csv"),
                                    fullPath => new CsvFile(fullPath));
        }

        private sealed class RowCountingVisitor : CsvReaderVisitorBase
        {
            private readonly Decoder _decoder = new UTF8Encoding(false, true).GetDecoder();

            public long RowCount { get; private set; }

            public override void VisitEndOfRecord() => ++this.RowCount;

            public unsafe override void VisitEndOfField(ReadOnlySpan<byte> chunk)
            {
#if NETCOREAPP
                _decoder.GetCharCount(chunk, true);
#else
                fixed (byte* b = &MemoryMarshal.GetReference(chunk))
                {
                    _decoder.GetCharCount(b == null ? (byte*)1 : b, chunk.Length, true);
                }
#endif
            }

            public unsafe override void VisitPartialFieldContents(ReadOnlySpan<byte> chunk)
            {
#if NETCOREAPP
                _decoder.GetCharCount(chunk, false);
#else
                if (!chunk.IsEmpty)
                {
                    fixed (byte* b = &MemoryMarshal.GetReference(chunk))
                    {
                        _decoder.GetCharCount(b, chunk.Length, false);
                    }
                }
#endif
            }
        }
    }
}
