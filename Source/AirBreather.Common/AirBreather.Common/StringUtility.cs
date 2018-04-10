using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

        public static byte[] HexStringToByteArrayChecked(this string s) =>
            hexStringRegex.Value.IsMatch(s.ValidateNotNull(nameof(s)))
                ? s.AsReadOnlySpan().HexStringToByteArrayUnchecked()
                : throw new ArgumentException("Must provide a hex string.", nameof(s));

        // http://stackoverflow.com/a/17923942/1083771
        public static byte[] HexStringToByteArrayUnchecked(this ReadOnlySpan<char> s)
        {
            if (s.Length == 0)
            {
                return Array.Empty<byte>();
            }

            unchecked
            {
                byte[] bytes = new byte[s.Length / 2];
                for (int i = 0; i < bytes.Length; ++i)
                {
                    int hi = s[i * 2] - 65;
                    hi = hi + 10 + ((hi >> 31) & 7);

                    int lo = s[i * 2 + 1] - 65;
                    lo = lo + 10 + ((lo >> 31) & 7) & 0x0f;

                    bytes[i] = unchecked((byte)(lo | hi << 4));
                }

                return bytes;
            }
        }

        // http://stackoverflow.com/a/24343727/1083771
        private static readonly uint[] byteToHex32Lookup = CreateLookup32Unsafe();

        private static uint[] CreateLookup32Unsafe()
        {
            uint[] result = new uint[256];
            for (int i = 0; i < result.Length; ++i)
            {
                string s = i.ToString("x2");
                result[i] = BitConverter.IsLittleEndian
                    ? s[0] | ((uint)s[1] << 16)
                    : s[1] | ((uint)s[0] << 16);
            }

            return result;
        }

        public static string ToHexString(this ReadOnlySpan<byte> bytes)
        {
            int cnt = bytes.Length;
            if (cnt == 0)
            {
                return String.Empty;
            }

            string result = new string(default, cnt * 2);
            ref byte bResult = ref Unsafe.As<char, byte>(ref Unsafe.AsRef(MemoryMarshal.GetReference(result.AsReadOnlySpan())));
            for (int i = 0; i < bytes.Length; i++)
            {
                Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bResult, new IntPtr(i * 4)),
                                      byteToHex32Lookup[bytes[i]]);
            }

            return result;
        }
    }
}
