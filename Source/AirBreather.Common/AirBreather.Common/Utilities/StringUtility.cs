using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace AirBreather.Common.Utilities
{
    public static unsafe class StringUtility
    {
        private static readonly Lazy<Regex> hexStringRegex = new Lazy<Regex>(() => new Regex("^([A-Fa-f0-9]{2})*$",
                                                                                             RegexOptions.CultureInvariant |
                                                                                             RegexOptions.Compiled |
                                                                                             RegexOptions.ExplicitCapture),
                                                                             LazyThreadSafetyMode.PublicationOnly);

        public static byte[] HexStringToByteArrayChecked(this string s)
        {
            s.ValidateNotNull(nameof(s));
            if (!hexStringRegex.Value.IsMatch(s))
            {
                throw new ArgumentException("Must provide a hex string.", nameof(s));
            }

            return s.HexStringToByteArrayUnchecked();
        }

        // http://stackoverflow.com/a/17923942/1083771
        public static byte[] HexStringToByteArrayUnchecked(this string s)
        {
            byte[] bytes = new byte[s.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int hi = s[i * 2] - 65;
                hi = hi + 10 + ((hi >> 31) & 7);

                int lo = s[i * 2 + 1] - 65;
                lo = lo + 10 + ((lo >> 31) & 7) & 0x0f;

                bytes[i] = (byte)(lo | hi << 4);
            }

            return bytes;
        }

        // http://stackoverflow.com/a/24343727/1083771
        private static readonly uint[] byteTo32BitLookupArray = CreateLookup32Unsafe();
        private static readonly uint* byteTo32BitLookupPtr = (uint*)GCHandle.Alloc(byteTo32BitLookupArray, GCHandleType.Pinned).AddrOfPinnedObject();

        private static uint[] CreateLookup32Unsafe()
        {
            uint[] result = new uint[256];
            for (int i = 0; i < result.Length; i++)
            {
                string s = i.ToString("X2");
                result[i] = BitConverter.IsLittleEndian
                    ? s[0] + ((uint)s[1] << 16)
                    : s[1] + ((uint)s[0] << 16);
            }

            return result;
        }

        public static unsafe string ByteArrayToHexString(this byte[] bytes)
        {
            uint* lookupP = byteTo32BitLookupPtr;
            string result = new string((char)0, bytes.Length * 2);

            fixed (byte* bytesP = bytes)
            fixed (char* resultP = result)
            {
                uint* resultP2 = (uint*)resultP;
                for (int i = 0; i < bytes.Length; i++)
                {
                    resultP2[i] = lookupP[bytesP[i]];
                }
            }

            return result;
        }
    }
}
