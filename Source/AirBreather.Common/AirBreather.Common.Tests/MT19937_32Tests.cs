using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

using AirBreather.Common.Random;

namespace AirBreather.Common.Tests
{
    public sealed class MT19937_32Tests
    {
        private readonly ITestOutputHelper output;

        public MT19937_32Tests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test()
        {
            // http://ideone.com/HWsvqS
            byte[] expectedBinary = Convert.FromBase64String("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");

            uint[] expectedResults = new uint[6000];
            Buffer.BlockCopy(expectedBinary, 0, expectedResults, 0, expectedBinary.Length);

            var gen = new MT19937_32Generator();
            var state = new MT19937_32State(5489);
            byte[] buf = new byte[expectedResults.Length * 4];

            // First, do it in separate calls.
            for (int i = 0; i < expectedResults.Length; i++)
            {
                state = gen.FillBuffer(state, buf, i * 4, 4);
                Assert.Equal(expectedResults[i], BitConverter.ToUInt32(buf, i * 4));
            }

            // Now, do it all in one call.
            state = new MT19937_32State(5489);
            buf = new byte[expectedResults.Length * 4];
            state = gen.FillBuffer(state, buf, 0, buf.Length);
            for (int i = 0; i < expectedResults.Length; i++)
            {
                Assert.Equal(expectedResults[i], BitConverter.ToUInt32(buf, i * 4));
            }

            // Now, repeat the same for the extension method.
            state = new MT19937_32State(5489);
            uint[] typedBuf = new uint[expectedResults.Length];
            for (int i = 0; i < expectedResults.Length; i++)
            {
                state = gen.FillBuffer(state, typedBuf, i, 1);
                Assert.Equal(expectedResults[i], typedBuf[i]);
            }

            // again, all in one call
            state = new MT19937_32State(5489);
            typedBuf = new uint[expectedResults.Length];
            state = gen.FillBuffer(state, typedBuf, 0, typedBuf.Length);
            for (int i = 0; i < expectedResults.Length; i++)
            {
                Assert.Equal(expectedResults[i], typedBuf[i]);
            }

            // Now, ensure that it throws if we're out of alignment.
            Assert.Throws<ArgumentException>("index", () => state = gen.FillBuffer(state, buf, 3, 4));
        }

        [Theory]
        [InlineData(0u, 1)]
        [InlineData(0u, 2)]
        [InlineData(0u, 4)]
        [InlineData(0u, 8)]
        [InlineData(0u, 16)]
        [InlineData(0u, 32)]
        [InlineData(0u, 64)]
        public void SpeedTestSingleArray(uint seed, int chunks)
        {
            var gen = new MT19937_32Generator();

            // stage 1: set up the initial state, output buffer, and chunk size.
            var initialState = new MT19937_32State(seed);

            const int OutputBufferLength = 1 << 30;
            var outputBuffer = new byte[OutputBufferLength];
            var chunkSize = OutputBufferLength / chunks;

            // stage 2: use that state to set up the parallel independent states.
            var parallelStateBuffer = new uint[chunks];
            gen.FillBuffer(initialState, parallelStateBuffer);

            var parallelStates = new MT19937_32State[chunks];
            for (int i = 0; i < parallelStates.Length; i++)
            {
                parallelStates[i] = new MT19937_32State(parallelStateBuffer[i]);
            }

            Stopwatch sw = Stopwatch.StartNew();

            // stage 3: do those chunks in parallel
            const int Reps = 3;
            for (int rep = 0; rep < Reps; rep++)
            {
                Parallel.For(0, chunks, i =>
                {
                    parallelStates[i] = gen.FillBuffer(parallelStates[i], outputBuffer, i * chunkSize, chunkSize);
                });
            }

            sw.Stop();

            double seconds = sw.ElapsedTicks / (double)Stopwatch.Frequency / (double)Reps;
            this.output.WriteLine("MT19937_32Tests.SpeedTestSingleArray: {0:N5} seconds, size of {1:N0} bytes ({2:N5} GiB per second), {3} separate chunk(s).",
                                  seconds,
                                  OutputBufferLength,
                                  OutputBufferLength / seconds / (1 << 30),
                                  chunks.ToString().PadLeft(2));

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        [Theory]
        [InlineData(0u, 1)]
        [InlineData(0u, 2)]
        [InlineData(0u, 4)]
        [InlineData(0u, 8)]
        [InlineData(0u, 16)]
        [InlineData(0u, 32)]
        [InlineData(0u, 64)]
        public void SpeedTestSeparateArraysWithMergeAtEnd(uint seed, int chunks)
        {
            var gen = new MT19937_32Generator();

            // stage 1: set up the initial state, output buffer, and chunk size.
            var initialState = new MT19937_32State(seed);

            const int OutputBufferLength = 1 << 30;
            var outputBuffer = new byte[OutputBufferLength];
            var chunkSize = OutputBufferLength / chunks;

            // stage 2: use that state to set up the parallel independent states.
            var parallelStateBuffer = new uint[chunks];
            gen.FillBuffer(initialState, parallelStateBuffer);

            var parallelStates = new MT19937_32State[chunks];
            for (int i = 0; i < parallelStates.Length; i++)
            {
                parallelStates[i] = new MT19937_32State(parallelStateBuffer[i]);
            }

            // stage 2.99: preallocate buffers for the different chunks.
            // doing "new byte[chunkSize]" inside stopwatch block would be unfair.
            byte[][] chunkBuffers = new byte[chunks][];
            for (int i = 0; i < chunkBuffers.Length; i++)
            {
                chunkBuffers[i] = new byte[chunkSize];
            }

            Stopwatch sw = Stopwatch.StartNew();

            // stage 3: do those chunks in parallel, with a serial merge.
            const int Reps = 3;
            for (int rep = 0; rep < Reps; rep++)
            {
                Parallel.For(0, chunks, i =>
                {
                    gen.FillBuffer(parallelStates[i], chunkBuffers[i]);

                    // it's actually permissible to do this now, but testing suggests that
                    // this is actually slower overall than doing the copies at the end.
                    ////Buffer.BlockCopy(chunkBuffers[i], 0, outputBuffer, i * chunkSize, chunkSize);
                });

                for (int i = 0; i < chunkBuffers.Length; i++)
                {
                    Buffer.BlockCopy(chunkBuffers[i], 0, outputBuffer, i * chunkSize, chunkSize);
                }
            }

            sw.Stop();

            // Note two things about this, compared to the other test.
            // Not only is it *slower* than writing to the single big buffer,
            // but it requires *greater* peak memory consumption overall.
            // HOWEVER, the other one has one particular disadvantage: it fixes
            // the *entire* array of 1 GiB for the duration.
            double seconds = sw.ElapsedTicks / (double)Stopwatch.Frequency / (double)Reps;
            this.output.WriteLine("MT19937_32Tests.SpeedTestSeparateArraysWithMergeAtEnd: {0:N5} seconds, size of {1:N0} bytes ({2:N5} GiB per second), {3} separate chunk(s).",
                                  seconds,
                                  OutputBufferLength,
                                  OutputBufferLength / seconds / (1 << 30),
                                  chunks.ToString().PadLeft(2));

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
