using System;
using System.IO;
using System.Text;

using AirBreather.Csv;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

using CsvHelper;
using CsvHelper.Configuration;

namespace AirBreather.Bench
{
    [MemoryDiagnoser]
    public class Program
    {
        private byte[] data;

        [GlobalSetup]
        public void Setup()
        {
            this.data = File.ReadAllBytes(@"N:\media\datasets\csv-from-nz\Building consents by institutional control (Monthly).csv");
        }

        [Benchmark(Baseline = true)]
        public long CountRowsUsingMine()
        {
            var visitor = new RowCountingVisitor();
            var tokenizer = new Rfc4180CsvTokenizer();
            tokenizer.ProcessNextReadBufferChunk(this.data, visitor);
            tokenizer.ProcessFinalReadBufferChunk(visitor);
            return visitor.RowCount;
        }

        [Benchmark]
        public long CountRowsUsingCsvHelper()
        {
            using (var ms = new MemoryStream(this.data, false))
            using (var tr = new StreamReader(ms, new UTF8Encoding(false, false), false))
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

        static void Main()
        {
            var prog = new Program();
            prog.Setup();
            Console.WriteLine(prog.CountRowsUsingMine());
            Console.WriteLine(prog.CountRowsUsingCsvHelper());

            BenchmarkRunner.Run<Program>(
                ManualConfig.Create(
                    DefaultConfig.Instance.With(
                        Job.Core.WithGcServer(true),
                        Job.Clr.WithGcServer(true))));
        }

        private sealed class RowCountingVisitor : CsvReaderVisitorBase
        {
            public long RowCount { get; private set; }

            public override void VisitEndOfLine() => ++this.RowCount;
            public override void VisitFieldData(ReadOnlySpan<byte> fieldData) { }
        }
    }
}
