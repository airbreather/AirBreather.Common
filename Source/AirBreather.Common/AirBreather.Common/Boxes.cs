using System.Runtime.CompilerServices;

namespace AirBreather
{
    public static class Boxes
    {
        private static readonly object TrueBox = true;
        private static readonly object FalseBox = false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Boolean(bool value) => value ? TrueBox : FalseBox;
    }
}
