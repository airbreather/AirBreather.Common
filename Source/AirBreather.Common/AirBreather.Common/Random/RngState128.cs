using System;

namespace AirBreather.Common.Random
{
    public struct RngState128 : IEquatable<RngState128>, IRandomGeneratorState
    {
        internal ulong s0; internal ulong s1;

        public RngState128(ulong s0, ulong s1)
        {
            if (0 == (s0 | s1))
            {
                throw new ArgumentException("At least one seed value must be non-zero.");
            }

            this.s0 = s0; this.s1 = s1;
        }

        public RngState128(RngState128 copyFrom)
            : this(copyFrom.s0, copyFrom.s1)
        {
        }

        public bool IsValid => StateIsValid(this);
        public ulong S0 => this.s0; public ulong S1 => this.s1;

        public static bool StateIsValid(RngState128 state) => 0 != (state.s0 | state.s1);

        public static bool Equals(RngState128 first, RngState128 second) => 0 == (first.s0 ^ second.s0 | (first.s1 ^ second.s1));
        public static int GetHashCode(RngState128 state) => (state.s0 ^ state.s1).GetHashCode();
        public static string ToString(RngState128 state) => FormattableString.Invariant($"{nameof(RngState128)}[S0 = {state.s0}, S1 = {state.s1}]");

        public static bool operator ==(RngState128 first, RngState128 second) => Equals(first, second);
        public static bool operator !=(RngState128 first, RngState128 second) => !Equals(first, second);
        public override bool Equals(object obj) => obj is RngState128 && Equals(this, (RngState128)obj);
        public bool Equals(RngState128 other) => Equals(this, other);
        public override int GetHashCode() => GetHashCode(this);
        public override string ToString() => ToString(this);
    }
}
