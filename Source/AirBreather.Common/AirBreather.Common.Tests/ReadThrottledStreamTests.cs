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

        [Fact]
        public void ReadShouldThrottle()
        {
            const int ThrottleRate = 1024;

            byte[] data = new byte[16384];
            CryptographicRandomGenerator.FillBuffer(data);

            byte[] finalBuf = new byte[data.Length];

            Stopwatch sw = Stopwatch.StartNew();
            using (var outputStream = new MemoryStream())
            {
                using (var readStream = new MemoryStream(data))
                using (var bufferedStream = new BufferedStream(readStream, 1024))
                using (var throttledStream = new ReadThrottledStream(bufferedStream, ThrottleRate))
                {
                    throttledStream.CopyTo(outputStream);
                }

                finalBuf = outputStream.ToArray();
            }

            sw.Stop();
            this.outputHelper.WriteLine("It took {0} seconds to copy {1} bytes at a nominal {2} bytes per second throttle.", sw.ElapsedTicks / (double)Stopwatch.Frequency, data.Length, ThrottleRate);
        }

        [Fact]
        public async Task ReadAsyncShouldThrottle()
        {
            const int ThrottleRate = 1024;

            byte[] data = new byte[16384];
            CryptographicRandomGenerator.FillBuffer(data);

            byte[] finalBuf = new byte[data.Length];

            Stopwatch sw = Stopwatch.StartNew();
            using (var outputStream = new MemoryStream())
            {
                using (var readStream = new MemoryStream(data))
                using (var bufferedStream = new BufferedStream(readStream, 1024))
                using (var throttledStream = new ReadThrottledStream(bufferedStream, ThrottleRate))
                {
                    await throttledStream.CopyToAsync(outputStream);
                }

                finalBuf = outputStream.ToArray();
            }

            sw.Stop();
            this.outputHelper.WriteLine("It took {0} seconds to copy {1} bytes at a nominal {2} bytes per second throttle.", sw.ElapsedTicks / (double)Stopwatch.Frequency, data.Length, ThrottleRate);
        }
    }
}
