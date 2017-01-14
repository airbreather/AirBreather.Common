using System.Runtime.CompilerServices;

namespace AirBreather.IO
{
    // the layout of decimal in-memory is not the same as how it get serialized on the wire.
    // NON-PORTABLE: Nehalem and above will handle unaligned reads not worse than we could (and
    // probably other microarchitectures in the x86 / x64 family as well), but apparently there's
    // this notion that some microarchitectures expect us to fix up unaligned reads in software.
    internal static class DecimalSerializationUtility
    {
        // layout of decimal in-memory: flags, hi, lo, mid
        internal static ref int dFlags(ref decimal d) => ref Unsafe.Add(ref Unsafe.As<decimal, int>(ref d), 0);
        internal static ref int dHi(ref decimal d)    => ref Unsafe.Add(ref Unsafe.As<decimal, int>(ref d), 1);
        internal static ref int dLo(ref decimal d)    => ref Unsafe.Add(ref Unsafe.As<decimal, int>(ref d), 2);
        internal static ref int dMid(ref decimal d)   => ref Unsafe.Add(ref Unsafe.As<decimal, int>(ref d), 3);

        // layout of decimal on the wire: lo, mid, hi, flags
        // These ones are ACTUALLY "unsafe", because the CLR probably won't stop you from passing in
        // managed pointers to byte variables that aren't embedded in something big enough, and so
        // in addition to being "unsafe", failures are probably going to be non-deterministic.
        // What's the "safe" alternative?  Span<T>, once that comes online... it should be able to
        // consistently reject invalid calls at least at runtime, and probably at compile time in
        // some cases too with the help of Roslyn analyzers that I'm sure someone will be writing.
        internal static ref int wLo(ref byte b0)    => ref Unsafe.Add(ref Unsafe.As<byte, int>(ref b0), 0);
        internal static ref int wMid(ref byte b0)   => ref Unsafe.Add(ref Unsafe.As<byte, int>(ref b0), 1);
        internal static ref int wHi(ref byte b0)    => ref Unsafe.Add(ref Unsafe.As<byte, int>(ref b0), 2);
        internal static ref int wFlags(ref byte b0) => ref Unsafe.Add(ref Unsafe.As<byte, int>(ref b0), 3);
    }
}
