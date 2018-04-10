using System;

using static System.FormattableString;

namespace AirBreather.Random
{
    public struct RngState128 : IEquatable<RngState128>, IRandomGeneratorState
    {
        public ulong S0; public ulong S1;

        public RngState128(ulong s0, ulong s1)
        {
            if (0 == (s0 | s1))
            {
                ThrowHelpers.ThrowArgumentExceptionForAllZeroes();
            }

            this.S0 = s0; this.S1 = s1;
        }

        public RngState128(RngState128 copyFrom)
            : this(copyFrom.S0, copyFrom.S1)
        {
        }

        public bool IsValid => 0 != (this.S0 | this.S1);

        public static bool operator ==(RngState128 first, RngState128 second) => first.Equals(second);
        public static bool operator !=(RngState128 first, RngState128 second) => !first.Equals(second);

        public override bool Equals(object obj) => obj is RngState128 other && Equals(this, other);
        public bool Equals(RngState128 other) => 0 == (this.S0 ^ other.S0 | (this.S1 ^ other.S1));
        public override int GetHashCode() => (this.S0 ^ this.S1).GetHashCode();
        public override string ToString() => Invariant($"{nameof(RngState128)}[S0 = {this.S0}, S1 = {this.S1}]");
    }
}
