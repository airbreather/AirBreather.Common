using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace AirBreather.Random
{
    public static class CryptographicRandomGenerator
    {
        // RNGCryptoServiceProvider claims to be thread-safe and carries no external state.
        private static readonly Lazy<RNGCryptoServiceProvider> SingletonProvider = new Lazy<RNGCryptoServiceProvider>();

        public static byte[] GetBuffer(int length)
        {
            byte[] result = new byte[length];
            FillBuffer(result);
            return result;
        }

        public static Span<byte> FillBuffer(Span<byte> data)
        {
            byte[] buf = ArrayPool<byte>.Shared.Rent(4096);
            try
            {
                ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(buf);
                int offset = 0;
                for (int end = data.Length - buf.Length; offset <= end; offset += buf.Length)
                {
                    SingletonProvider.Value.GetBytes(buf);
                    span.CopyTo(data.Slice(offset, buf.Length));
                }

                int rem = data.Length - offset;
                if (rem != 0)
                {
                    SingletonProvider.Value.GetBytes(buf, 0, rem);
                    span.Slice(0, rem).CopyTo(data.Slice(offset, rem));
                }

                return data;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buf, clearArray: true);
            }
        }

        // Offer quick-and-easy ways of generating random value types, because this is often going
        // to be used just to get a random seed for a PRNG.
        public static byte NextByte() => Next<byte>();
        public static short NextInt16() => Next<short>();
        public static int NextInt32() => Next<int>();
        public static long NextInt64() => Next<long>();

        public static sbyte NextSByte() => Next<sbyte>();
        public static ushort NextUInt16() => Next<ushort>();
        public static uint NextUInt32() => Next<uint>();
        public static ulong NextUInt64() => Next<ulong>();

        public static double NextDouble() => NextUInt64() / (double)UInt64.MaxValue;
        public static float NextSingle() => (float)NextDouble();

        private static T Next<T>()
        {
            Span<byte> buf = stackalloc byte[Unsafe.SizeOf<T>()];
            FillBuffer(buf);
            return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(buf));
        }
    }
}
