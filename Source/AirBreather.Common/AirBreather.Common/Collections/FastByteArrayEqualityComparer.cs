using System.Collections.Generic;

using AirBreather.Common.Utilities;

namespace AirBreather.Common.Collections
{
    // based on http://stackoverflow.com/a/8808245/1083771
    public sealed class FastByteArrayEqualityComparer : EqualityComparer<byte[]>
    {
        // stateless, so this is OK.
        public static readonly FastByteArrayEqualityComparer Instance = new FastByteArrayEqualityComparer();

        public unsafe override bool Equals(byte[] a1, byte[] a2)
        {
            if (a1 == a2)
                return true;
            if (a1 == null || a2 == null || a1.Length != a2.Length)
                return false;
            fixed (byte* p1 = a1, p2 = a2)
            {
                byte* x1 = p1, x2 = p2;
                int l = a1.Length;
                for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                    if (*((long*)x1) != *((long*)x2)) return false;
                if ((l & 4) != 0) { if (*((int*)x1) != *((int*)x2)) return false; x1 += 4; x2 += 4; }
                if ((l & 2) != 0) { if (*((short*)x1) != *((short*)x2)) return false; x1 += 2; x2 += 2; }
                if ((l & 1) != 0) if (*x1 != *x2) return false;
                return true;
            }
        }

        public unsafe override int GetHashCode(byte[] obj)
        {
            if (obj == null)
            {
                return 0;
            }

            int result = HashCodeUtility.Seed;

            int l = obj.Length;
            result = result.HashWith(l);

            if (l < 8)
            {
                for (int i = 0; i < obj.Length; i++)
                {
                    result = result.HashWith(obj[i]);
                }

                return result;
            }

            fixed (byte* p = obj)
            {
                long* x = (long*)p;
                long* xEnd = (long*)p + l - 8;
                int step = 1;

                while (x < xEnd)
                {
                    result = result.HashWith(*x);
                    x += step++;
                }

                return result;
            }
        }
    }
}
