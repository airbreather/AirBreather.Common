using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Xunit;

using AirBreather.Common.Random;

namespace AirBreather.Common.Tests
{
    public sealed class XorShift1024StarTests
    {
        // I, too, like to live dangerously.
        private const int pOffset = sizeof(ulong) * 16;

        [Theory]
        [InlineData(1234524356ul, 47845723665ul, 1)]
        [InlineData(1234524356ul, 47845723665ul, 2)]
        [InlineData(1234524356ul, 47845723665ul, 4)]
        [InlineData(1234524356ul, 47845723665ul, 8)]
        [InlineData(1234524356ul, 47845723665ul, 16)]
        [InlineData(1234524356ul, 47845723665ul, 32)]
        [InlineData(1234524356ul, 47845723665ul, 64)]
        public unsafe void SpeedTestSingleArray(ulong s0, ulong s1, int chunks)
        {
            // see how quickly we can fill the biggest possible buffer that doesn't require 2GB array sizes.
            var gen = new XorShift1024StarGenerator();

            // stage 1: set up the initial state, output buffer, and chunk size.
            XorShift1024StarState initialState = CreateInitialState(s0, s1);

            const int OutputBufferLength = 1 << 30;
            var outputBuffer = new byte[OutputBufferLength];
            var chunkSize = OutputBufferLength / chunks;

            // stage 2: use that state to set up the parallel independent states.
            var parallelStateBuffer = new byte[sizeof(XorShift1024StarState) * chunks];
            gen.FillBuffer(initialState, parallelStateBuffer);

            var parallelStates = new XorShift1024StarState[chunks];
            fixed (byte* sFixed = parallelStateBuffer)
            fixed (XorShift1024StarState* tFixed = parallelStates)
            {
                var s = (XorShift1024StarState*)sFixed;
                var sEnd = s + chunks;
                var t = tFixed;

                while (s < sEnd)
                {
                    // zero out "p", as it can only be 0-15.
                    *((int*)(((byte*)s) + pOffset)) = 0;
                    *(t++) = *(s++);
                }
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
            Console.WriteLine("XorShift1024StarTests.SpeedTestSingleArray: {0:N5} seconds, size of {1:N0} bytes ({2:N5} GiB per second), {3} separate chunk(s).",
                              seconds,
                              OutputBufferLength,
                              OutputBufferLength / seconds / (1 << 30),
                              chunks.ToString().PadLeft(2));

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        [Theory]
        [InlineData(1234524356ul, 47845723665ul, 1)]
        [InlineData(1234524356ul, 47845723665ul, 2)]
        [InlineData(1234524356ul, 47845723665ul, 4)]
        [InlineData(1234524356ul, 47845723665ul, 8)]
        [InlineData(1234524356ul, 47845723665ul, 16)]
        [InlineData(1234524356ul, 47845723665ul, 32)]
        [InlineData(1234524356ul, 47845723665ul, 64)]
        public unsafe void SpeedTestSeparateArraysWithMergeAtEnd(ulong s0, ulong s1, int chunks)
        {
            // see how quickly we can fill the biggest possible buffer that doesn't require 2GB array sizes.
            var gen = new XorShift1024StarGenerator();

            // stage 1: set up the initial state, output buffer, and chunk size.
            XorShift1024StarState initialState = CreateInitialState(s0, s1);

            const int OutputBufferLength = 1 << 30;
            var outputBuffer = new byte[OutputBufferLength];
            var chunkSize = OutputBufferLength / chunks;

            // stage 2: use that state to set up the parallel independent states.
            var parallelStateBuffer = new byte[sizeof(XorShift1024StarState) * chunks];
            gen.FillBuffer(initialState, parallelStateBuffer);

            var parallelStates = new XorShift1024StarState[chunks];
            fixed (byte* sFixed = parallelStateBuffer)
            fixed (XorShift1024StarState* tFixed = parallelStates)
            {
                var s = (XorShift1024StarState*)sFixed;
                var sEnd = s + chunks;
                var t = tFixed;

                while (s < sEnd)
                {
                    // zero out "p", as it can only be 0-15.
                    *((int*)(((byte*)s) + pOffset)) = 0;
                    *(t++) = *(s++);
                }
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
            // the *entire* array of just under 2 GiB for the duration.
            double seconds = sw.ElapsedTicks / (double)Stopwatch.Frequency / (double)Reps;
            Console.WriteLine("XorShift1024StarTests.SpeedTestSeparateArraysWithMergeAtEnd: {0:N5} seconds, size of {1:N0} bytes ({2:N5} GiB per second), {3} separate chunk(s).",
                              seconds,
                              OutputBufferLength,
                              OutputBufferLength / seconds / (1 << 30),
                              chunks.ToString().PadLeft(2));

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private static unsafe XorShift1024StarState CreateInitialState(ulong s0, ulong s1)
        {
            // Use xorshift128+ to generate the state for xorshift1024*.
            var gen = new XorShift128PlusGenerator();
            var state = new XorShift128PlusState(s0, s1);

            byte[] buf = new byte[sizeof(ulong) * 16 + Math.Max(sizeof(int), XorShift128PlusGenerator.ChunkSize)];
            gen.FillBuffer(state, buf);

            fixed (byte* fBuf = buf)
            {
                // zero out "p", as it can only be 0-15.
                *((int*)(fBuf + pOffset)) = 0;
                return *((XorShift1024StarState*)fBuf);
            }
        }
    }
}
