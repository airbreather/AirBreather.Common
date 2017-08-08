using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AirBreather
{
    public static class StreamUtility
    {
        // Read exactly the number of bytes, unless the stream doesn't have enough data.  None of
        // this "I can possibly return fewer bytes than requested even if I'm not done" crap.
        public static async Task<int> LoopedReadAsync(this Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            // others get validated during the actual call.
            stream.ValidateNotNull(nameof(stream));

            int orig = count;
            int cnt = -1;
            while ((count != 0) & (cnt != 0))
            {
                cnt = await stream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
                offset += cnt;
                count -= cnt;
            }

            return orig - count;
        }
    }
}
