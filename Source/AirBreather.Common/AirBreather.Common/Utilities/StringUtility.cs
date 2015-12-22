namespace AirBreather.Common.Utilities
{
    public static class StringUtility
    {
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
    }
}
