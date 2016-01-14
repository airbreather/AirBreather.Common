using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

using AirBreather.Common.IO;
using AirBreather.Common.Random;
using AirBreather.Common.Utilities;

namespace AirBreather.Common.Tests
{
    public sealed class ReadThrottledStreamTests
    {
        private readonly ITestOutputHelper outputHelper;

        public ReadThrottledStreamTests(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper.ValidateNotNull(nameof(outputHelper));
        }

        [Theory]
        [InlineData(1024)]
        [InlineData(2005)]
        [InlineData(3089)]
        [InlineData(9999)]
        [InlineData(16383)]
        [InlineData(16384)]
        [InlineData(16385)]
        [InlineData(99999)]
        [InlineData(Int32.MaxValue)]
        public void ReadShouldThrottle(int throttleRateBytesPerSecond)
        {
            byte[] data = new byte[16384];
            CryptographicRandomGenerator.FillBuffer(data);

            byte[] finalBuf = new byte[data.Length];

            Stopwatch sw = Stopwatch.StartNew();
            using (var outputStream = new MemoryStream())
            {
                using (var readStream = new MemoryStream(data))
                using (var bufferedStream = new BufferedStream(readStream, 1024))
                using (var throttledStream = new ReadThrottledStream(bufferedStream, throttleRateBytesPerSecond))
                {
                    throttledStream.CopyTo(outputStream);
                }

                finalBuf = outputStream.ToArray();
            }

            sw.Stop();
            double seconds = sw.ElapsedTicks / (double)Stopwatch.Frequency;
            this.outputHelper.WriteLine("It took {0} seconds to copy {1} bytes at a nominal {2} bytes per second throttle, for a real {3} bytes per second throttle.", seconds, data.Length, throttleRateBytesPerSecond, data.Length / seconds);
        }

        [Theory]
        [InlineData(1024)]
        [InlineData(2005)]
        [InlineData(3089)]
        [InlineData(9999)]
        [InlineData(16383)]
        [InlineData(16384)]
        [InlineData(16385)]
        [InlineData(99999)]
        [InlineData(Int32.MaxValue)]
        public async Task ReadAsyncShouldThrottle(int throttleRateBytesPerSecond)
        {
            byte[] data = new byte[16384];
            CryptographicRandomGenerator.FillBuffer(data);

            byte[] finalBuf = new byte[data.Length];

            Stopwatch sw = Stopwatch.StartNew();
            using (var outputStream = new MemoryStream())
            {
                using (var readStream = new MemoryStream(data))
                using (var bufferedStream = new BufferedStream(readStream, 1024))
                using (var throttledStream = new ReadThrottledStream(bufferedStream, throttleRateBytesPerSecond))
                {
                    await throttledStream.CopyToAsync(outputStream);
                }

                finalBuf = outputStream.ToArray();
            }

            sw.Stop();
            double seconds = sw.ElapsedTicks / (double)Stopwatch.Frequency;
            this.outputHelper.WriteLine("It took {0} seconds to copy {1} bytes at a nominal {2} bytes per second throttle, for a real {3} bytes per second throttle.", seconds, data.Length, throttleRateBytesPerSecond, data.Length / seconds);
        }
    }
}
