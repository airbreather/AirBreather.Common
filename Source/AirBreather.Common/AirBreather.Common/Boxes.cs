using System.Runtime.CompilerServices;

using AirBreather.Common.Utilities;

namespace AirBreather.Common
{
    public static class Boxes
    {
        private static readonly object TrueBox = true;
        private static readonly object FalseBox = false;
        private static readonly object[] IntBoxes =
        {
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Boolean(bool value) => value ? TrueBox : FalseBox;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Int32(int value) => value.IsInRange(0, IntBoxes.Length) ? IntBoxes[value] : value;
    }
}
