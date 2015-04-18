using AirBreather.Core.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AirBreather.Core.Tests
{
    public sealed class UInt32SetTests
    {
        [Fact]
        public void Test()
        {
            const int ValueCount = 900000;
            const int TestCount = 50000000;
            uint[] vals;
            uint[] testVals;

            {
                byte[] valData = new byte[ValueCount * 4];
                var rand = new System.Random(Guid.NewGuid().GetHashCode());
                rand.NextBytes(valData);
                vals = new uint[ValueCount];
                Buffer.BlockCopy(valData, 0, vals, 0, valData.Length);
            }

            {
                byte[] valData = new byte[TestCount * 4];
                var rand = new System.Random(Guid.NewGuid().GetHashCode());
                rand.NextBytes(valData);
                testVals = new uint[TestCount];
                Buffer.BlockCopy(valData, 0, testVals, 0, valData.Length);
            }

            UInt32Set mySet = new UInt32Set(vals);
            HashSet<uint> realSet = new HashSet<uint>(vals);

            byte[] myResults = new byte[TestCount];
            byte[] realResults = new byte[TestCount];

            const byte TrueByte = 1;
            const byte FalseByte = 0;

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < testVals.Length; i++)
            {
                myResults[i] = mySet.Contains(testVals[i]) ? TrueByte : FalseByte;
            }

            sw.Stop();

            long mySetTicks = sw.ElapsedTicks;

            sw.Restart();
            for (int i = 0; i < testVals.Length; i++)
            {
                realResults[i] = realSet.Contains(testVals[i]) ? TrueByte : FalseByte;
            }

            sw.Stop();

            ////System.IO.File.AppendAllText(@"C:\Freedom\mine.txt", String.Format("mineSet: {0} seconds", mySetTicks / (double)Stopwatch.Frequency));
            ////System.IO.File.AppendAllText(@"C:\Freedom\mine.txt", Environment.NewLine);
            ////System.IO.File.AppendAllText(@"C:\Freedom\mine.txt", String.Format("realSet: {0} seconds", sw.ElapsedTicks / (double)Stopwatch.Frequency));
            ////System.IO.File.AppendAllText(@"C:\Freedom\mine.txt", Environment.NewLine);

            Assert.True(UnsafeCompare(realResults, myResults));
        }

        // http://stackoverflow.com/a/8808245/1083771
        static unsafe bool UnsafeCompare(byte[] a1, byte[] a2)
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
                if ((l & 1) != 0) if (*((byte*)x1) != *((byte*)x2)) return false;
                return true;
            }
        }
    }
}
