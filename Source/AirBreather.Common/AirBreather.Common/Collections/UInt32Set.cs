using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AirBreather.Common.Collections
{
    // There's absolutely no reason to use this.  HashSet<uint> is fast enough and lean enough.
    // Optimizations over HashSet<uint>:
    // - needs between 1 and 3 fewer objects allocated on the heap
    // - Contains takes just 75% of the time
    // - Approximately 24 bytes smaller, depending on system
    // Disadvantages:
    // - uint only
    // - only supports Contains checks, nothing else (so in other words, "get me all the things contained in this set" would be "for every valid UInt32 value, if this set contains it, add that to a regular list" and then return that list)
    // - zero error handling (though if you really really need the tiny speed and memory boost, I think you'll live)
    public struct UInt32Set
    {
        // Layout:
        // [value1, next1, value2, next2, ..., valueN, nextN, bucket1, bucket2, ..., bucketN]
        private uint[] data;

#if INSANE_MEMORY
        private uint Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (uint)(data.Length / 3); }
        }

        private uint BucketStart
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Length * 2; }
        }
#else
        private uint Length;
        private uint BucketStart;
#endif

#if INSANE_MEMORY
        public UInt32Set(uint[] collection)
        {
            ////if (collection == null)
            ////{
            ////    throw new ArgumentNullException("collection");
            ////}

            uint count = (uint)collection.Length;
            uint length = GetPrime(count);
            ////if (UInt32.MaxValue / 3 < Length)
            ////{
            ////    throw new NotSupportedException("way too long");
            ////}

            data = new uint[length * 3];

            for (uint i = 0; i < count; i++)
            {
                uint value = collection[i];
                uint bucket = value % length;
                data[i * 2 + ValsOffset] = value;
                data[i * 2 + NextOffset] = unchecked(data[length + length + bucket] - 1);
                data[length + length + bucket] = i + 1;
            }
        }
#else
        public UInt32Set(uint[] collection)
        {
            ////if (collection == null)
            ////{
            ////    throw new ArgumentNullException("collection");
            ////}

            Length = GetPrime((uint)collection.Length);
            ////if (UInt32.MaxValue / 3 < Length)
            ////{
            ////    throw new NotSupportedException("way too long");
            ////}

            data = new uint[Length * 3];
            BucketStart = Length << 1;

            for (uint i = 0; i < collection.Length; i++)
            {
                uint value = collection[i];
                uint bucket = value % Length;
                data[i * 2] = value;
                data[i * 2 + 1] = unchecked(data[BucketStart + bucket] - 1);
                data[BucketStart + bucket] = i + 1;
            }
        }
#endif

        public bool Contains(uint item)
        {
            ////if (this.buckets == null)
            ////{
            ////    return false;
            ////}

            for (uint i = unchecked(data[BucketStart + (item % Length)] - 1); i != UInt32.MaxValue; i = data[i + 1])
            {
                i <<= 1;
                if (data[i] == item)
                {
                    return true;
                }
            }

            return false;
        }

        // https://github.com/Microsoft/referencesource/blob/9da503f9ef21e8d1f2905c78d4e3e5cbb3d6f85a/mscorlib/system/collections/hashtable.cs#L1691-L1878
        #region HashHelpers

        private static readonly uint[] Primes = {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369};

        private static uint GetPrime(uint min)
        {
            for (uint i = 0; i < Primes.Length; i++)
            {
                uint prime = Primes[i];
                if (prime >= min)
                {
                    return prime;
                }
            }

            for (uint i = (min | 1); i < UInt32.MaxValue; i += 2)
            {
                // The % 101 in the base version strictly benefits Hashtable.
                ////if (IsPrime(i) && ((i - 1) % 101 != 0))
                if (IsPrime(i))
                {
                    return i;
                }
            }

            return min;
        }

        private static bool IsPrime(uint candidate)
        {
            if ((candidate & 1) == 0)
            {
                return candidate == 2;
            }

            uint limit = (uint)Math.Sqrt(candidate);
            for (uint divisor = 3; divisor <= limit; divisor += 2)
            {
                if ((candidate % divisor) == 0)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}