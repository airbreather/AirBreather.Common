using System;

namespace AirBreather.BinaryHash
{
    public static class Murmur3_32
    {
        // https://en.wikipedia.org/wiki/MurmurHash#Algorithm
        public static unsafe uint Hash(ReadOnlySpan<byte> data, uint seed = 0)
        {
            unchecked
            {
                const uint C1 = 0xcc9e2d51;
                const uint C2 = 0x1b873593;
                const int R1 = 15;
                const int R2 = 13;
                const uint M = 5;
                const uint N = 0xe6546b64;

                // language has no ROL operator
                const int R1C = 32 - R1;
                const int R2C = 32 - R2;

                ReadOnlySpan<uint> chunkedData = data.NonPortableCast<byte, uint>();
                ReadOnlySpan<byte> residue = data.Slice(chunkedData.Length << 2);
                uint h = seed;

                fixed (uint* fChunkedData = &chunkedData.DangerousGetPinnableReference())
                {
                    uint* pCur = fChunkedData;
                    uint* pEnd = fChunkedData + chunkedData.Length;

                    // hash the variable section of data, one 4-byte chunk at a time.
                    while (pCur != pEnd)
                    {
                        uint k = *(pCur++);
                        k *= C1;
                        k = (k << R1) | (k >> R1C); // k = k ROL R1
                        k *= C2;

                        h ^= k;
                        h = (h << R2) | (h >> R2C); // h = h ROL R2
                        h = (h * M) + N;
                    }
                }

                // handle the last incomplete chunk, if any.
                uint k2 = 0;
                switch (residue.Length)
                {
                    case 3:
                        k2 = (uint)residue[2] << 16;
                        goto case 2;

                    case 2:
                        k2 ^= (uint)residue[1] << 8;
                        goto case 1;

                    case 1:
                        k2 ^= residue[0];
                        k2 *= C1;
                        k2 = (k2 << R1) | (k2 >> R1C); // k2 = k2 ROL R1
                        k2 *= C2;
                        h ^= k2;
                        break;
                }

                // finalize
                h ^= (uint)data.Length;
                h ^= h >> 16;
                h *= 0x85ebca6b;
                h ^= h >> 13;
                h *= 0xc2b2ae35;
                h ^= h >> 16;

                return h;
            }
        }
    }
}
