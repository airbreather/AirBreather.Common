using System;
using System.Runtime.InteropServices;

namespace AirBreather
{
    internal static class UnsafeNativeMethods
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern int memcmp(byte* b1, byte* b2, IntPtr count);
    }
}
