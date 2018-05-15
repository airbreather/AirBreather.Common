using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AirBreather
{
    public static unsafe class StringUtility
    {
        public static byte[] HexStringToByteArrayChecked(this string s)
        {
            s.ValidateNotNull(nameof(s));
            foreach (char c in s)
            {
                if (!IsHex(c))
                {
                    ThrowHelpers.ThrowArgumentException("Must provide a hex string.", nameof(s));
                }
            }

            return s.AsSpan().HexStringToByteArrayUnchecked();
            bool IsHex(char c)
            {
                // at the time of writing, on both x86 and x64, the assembly for this method is 1
                // byte smaller than the assembly for 3 range checks, it has 2 fewer conditional
                // jumps, and the larger 0-9 case is tighter (early return instead of jump to end).
                if ('0' <= c && c <= '9')
                {
                    return true;
                }

                c = unchecked((char)(c & 0xFFDF));
                return ('A' <= c && c <= 'F');
            }
        }

        // http://stackoverflow.com/a/17923942/1083771
        public static byte[] HexStringToByteArrayUnchecked(this ReadOnlySpan<char> s)
        {
            if (s.Length == 0)
            {
                return Array.Empty<byte>();
            }

            byte[] bytes = new byte[s.Length / 2];
            CopyHexStringToByteArrayUnchecked(s, bytes);
            return bytes;
        }

        public static void CopyHexStringToByteArrayUnchecked(this ReadOnlySpan<char> s, Span<byte> b)
        {
            // this isn't really the kind of "check" that "checked / unchecked" was made for... that
            // was more "throw if this string isn't *actually* a hex string".
            if (s.Length != b.Length * 2)
            {
                ThrowHelpers.ThrowArgumentException("Hex string must have 2 chars for every desired output byte.");
            }

            unchecked
            {
                for (int i = 0; i < b.Length; ++i)
                {
                    int hi = s[i * 2] - 65;
                    hi = hi + 10 + ((hi >> 31) & 7);

                    int lo = s[i * 2 + 1] - 65;
                    lo = lo + 10 + ((lo >> 31) & 7) & 0x0f;

                    b[i] = unchecked((byte)(lo | hi << 4));
                }
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

        public static unsafe string ToHexString(this ReadOnlySpan<byte> bytes)
        {
            int cnt = bytes.Length;
            if (cnt == 0)
            {
                return String.Empty;
            }

            string result = new string(default, cnt * 2);
            fixed (char* c = result)
            {
                Span<char> span = new Span<char>(c, result.Length);
                CopyToHexString(bytes, span);
            }

            return result;
        }

        public static void CopyToHexString(this ReadOnlySpan<byte> bytes, Span<char> target)
        {
            if (target.Length != bytes.Length * 2)
            {
                ThrowHelpers.ThrowArgumentExceptionForBadTargetSpanLength();
            }

            ref byte bResult = ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(target));
            for (int i = 0; i < bytes.Length; i++)
            {
                Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bResult, new IntPtr(i * 4)),
                                      byteToHex32Lookup[bytes[i]]);
            }
        }
    }
}
