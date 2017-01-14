using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AirBreather.Text
{
    internal static class Helpers
    {
        internal static IEnumerator<char> CreateCharEnumerator(ArraySegment<byte> encodedData, Encoding encoding)
        {
            var decoder = encoding.GetDecoder();
            bool completed = false;
            byte[] arr = encodedData.Array;
            int i = encodedData.Offset;
            int cnt = encodedData.Count;

            // TODO: tmpBuf really ought to use a native buffer pool instead of one that uses the
            // managed heap, considering that we keep pinning it in spurts, we always refer to its
            // contents as char, and we have to do a bit of manual memory management anyway.
            const int MaxChar = 256;
            byte[] tmpBuf = ByteArrayPool.Instance.Rent(MaxChar * 2);
            try
            {
                while (!completed)
                {
                    // work around semi-arbitrary "no unsafe code in iterator methods" limitation by
                    // just defining a local method right here that we immediately call, which is
                    // actually perfectly "safe" in this usage.
                    Decode(decoder, arr, ref i, ref cnt, tmpBuf, MaxChar, out var charsUsed, out completed);
                    unsafe void Decode(Decoder decoder2, byte[] arr2, ref int i2, ref int cnt2, byte[] tmpBuf2, int maxChar2, out int charsUsed2, out bool completed2)
                    {
                        int bytesUsed;
                        fixed (byte* fromPtr = &arr2[i2], toPtr = tmpBuf2)
                        {
                            decoder2.Convert(fromPtr, cnt2, (char*)toPtr, maxChar2, false, out bytesUsed, out charsUsed2, out completed2);
                        }

                        i2 += bytesUsed;
                        cnt2 -= bytesUsed;
                    }

                    for (int j = 0; j < charsUsed; j++)
                    {
                        yield return Unsafe.As<byte, char>(ref tmpBuf[j]);
                    }
                }
            }
            finally
            {
                ByteArrayPool.Instance.Return(tmpBuf);
            }
        }
    }
}
