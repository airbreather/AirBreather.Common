using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace AirBreather
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
            if (s.Length == 0)
            {
                return Array.Empty<byte>();
            }

            byte[] bytes = new byte[s.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int hi = s[i * 2] - 65;
                hi = hi + 10 + ((hi >> 31) & 7);

                int lo = s[i * 2 + 1] - 65;
                lo = lo + 10 + ((lo >> 31) & 7) & 0x0f;

                bytes[i] = unchecked((byte)(lo | hi << 4));
            }

            return bytes;
        }

        // http://stackoverflow.com/a/24343727/1083771
        private static readonly uint[] byteToHex32Lookup = CreateLookup32Unsafe();

        private static uint[] CreateLookup32Unsafe()
        {
            uint[] result = new uint[256];
            for (int i = 0; i < result.Length; i++)
            {
                string s = i.ToString("x2");
                result[i] = BitConverter.IsLittleEndian
                    ? s[0] | ((uint)s[1] << 16)
                    : s[1] | ((uint)s[0] << 16);
            }

            return result;
        }

        public static unsafe string ByteArrayToHexString(this byte[] bytes)
        {
            if (bytes.ValidateNotNull(nameof(bytes)).Length == 0)
            {
                return String.Empty;
            }

            fixed (byte* bytesPtr = bytes)
            {
                return BytesToHexStringUnsafeCore(bytesPtr, bytes.Length);
            }
        }

        public static unsafe string BytesToHexStringUnsafe(byte* bytes, int cnt) => cnt.ValidateNotLessThan(nameof(cnt), 0) == 0
            ? String.Empty
            : BytesToHexStringUnsafeCore(bytes, cnt);

        private static unsafe string BytesToHexStringUnsafeCore(byte* bytes, int cnt)
        {
            string result = new string(default(char), cnt * 2);
            fixed (uint* byteToHexPtr = byteToHex32Lookup)
            fixed (char* resultCharPtr = result)
            {
                uint* resultUInt32Ptr = (uint*)resultCharPtr;
                for (int i = 0; i < cnt; i++)
                {
                    resultUInt32Ptr[i] = byteToHexPtr[bytes[i]];
                }
            }

            return result;
        }
    }
}
