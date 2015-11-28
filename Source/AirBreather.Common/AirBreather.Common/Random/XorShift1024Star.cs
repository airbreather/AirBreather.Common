using System;
using System.Diagnostics.CodeAnalysis;

using AirBreather.Common.Utilities;

namespace AirBreather.Common.Random
{
    public struct XorShift1024StarState : IEquatable<XorShift1024StarState>, IRandomGeneratorState
    {
        internal ulong s0, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14, s15;
        internal int p;

        public XorShift1024StarState(ulong s0, ulong s1, ulong s2, ulong s3, ulong s4, ulong s5, ulong s6, ulong s7, ulong s8, ulong s9, ulong s10, ulong s11, ulong s12, ulong s13, ulong s14, ulong s15)
            : this(s0, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14, s15, 0)
        {
        }

        public XorShift1024StarState(XorShift1024StarState copyFrom)
            : this(copyFrom.s0, copyFrom.s1, copyFrom.s2, copyFrom.s3, copyFrom.s4, copyFrom.s5, copyFrom.s6, copyFrom.s7, copyFrom.s8, copyFrom.s9, copyFrom.s10, copyFrom.s11, copyFrom.s12, copyFrom.s13, copyFrom.s14, copyFrom.s15, copyFrom.p)
        {
        }

        private XorShift1024StarState(ulong s0, ulong s1, ulong s2, ulong s3, ulong s4, ulong s5, ulong s6, ulong s7, ulong s8, ulong s9, ulong s10, ulong s11, ulong s12, ulong s13, ulong s14, ulong s15, int p)
        {
            if (0 == (s0 | s1 | s2 | s3 | s4 | s5 | s6 | s7 | s8 | s9 | s10 | s11 | s12 | s13 | s14 | s15))
            {
                throw new ArgumentException("At least one seed value must be non-zero.");
            }

            p.ValidateInRange(nameof(p), 0, 16);

            this.s0 = s0; this.s1 = s1; this.s2 = s2; this.s3 = s3; this.s4 = s4; this.s5 = s5; this.s6 = s6; this.s7 = s7; this.s8 = s8; this.s9 = s9; this.s10 = s10; this.s11 = s11; this.s12 = s12; this.s13 = s13; this.s14 = s14; this.s15 = s15;
            this.p = p;
        }

        public bool IsValid => StateIsValid(this);
        public ulong S0 => this.s0; public ulong S1 => this.s1; public ulong S2 => this.s2; public ulong S3 => this.s3; public ulong S4 => this.s4; public ulong S5 => this.s5; public ulong S6 => this.s6; public ulong S7 => this.s7; public ulong S8 => this.s8; public ulong S9 => this.s9; public ulong S10 => this.s10; public ulong S11 => this.s11; public ulong S12 => this.s12; public ulong S13 => this.s13; public ulong S14 => this.s14; public ulong S15 => this.s15;
        public int P => this.p;

        public static bool StateIsValid(XorShift1024StarState state) => state.p.IsInRange(0, 16) && 0 != (state.s0 | state.s1 | state.s2 | state.s3 | state.s4 | state.s5 | state.s6 | state.s7 | state.s8 | state.s9 | state.s10 | state.s11 | state.s12 | state.s13 | state.s14 | state.s15);

        public static bool Equals(XorShift1024StarState first, XorShift1024StarState second) => first.p == second.p && 0 == ((first.s0 ^ second.s0) | (first.s1 ^ second.s1) | (first.s2 ^ second.s2) | (first.s3 ^ second.s3) | (first.s4 ^ second.s4) | (first.s5 ^ second.s5) | (first.s6 ^ second.s6) | (first.s7 ^ second.s7) | (first.s8 ^ second.s8) | (first.s9 ^ second.s9) | (first.s10 ^ second.s10) | (first.s11 ^ second.s11) | (first.s12 ^ second.s12) | (first.s13 ^ second.s13) | (first.s14 ^ second.s14) | (first.s15 ^ second.s15));
        public static int GetHashCode(XorShift1024StarState state) => (state.s0 ^ state.s1 ^ state.s2 ^ state.s3 ^ state.s4 ^ state.s5 ^ state.s6 ^ state.s7 ^ state.s8 ^ state.s9 ^ state.s10 ^ state.s11 ^ state.s12 ^ state.s13 ^ state.s14 ^ state.s15 ^ unchecked((uint)state.p)).GetHashCode();
        public static string ToString(XorShift1024StarState state) => ToStringUtility.Begin(state).AddProperty("S0", state.s0).AddProperty("S1", state.s1).AddProperty("S2", state.s2).AddProperty("S3", state.s3).AddProperty("S4", state.s4).AddProperty("S5", state.s5).AddProperty("S6", state.s6).AddProperty("S7", state.s7).AddProperty("S8", state.s8).AddProperty("S9", state.s9).AddProperty("S10", state.s10).AddProperty("S11", state.s11).AddProperty("S12", state.s12).AddProperty("S13", state.s13).AddProperty("S14", state.s14).AddProperty("S15", state.s15).AddProperty("P", state.p).End();

        public static bool operator ==(XorShift1024StarState first, XorShift1024StarState second) => Equals(first, second);
        public static bool operator !=(XorShift1024StarState first, XorShift1024StarState second) => !Equals(first, second);
        public override bool Equals(object obj) => obj is XorShift1024StarState && Equals(this, (XorShift1024StarState)obj);
        public bool Equals(XorShift1024StarState other) => Equals(this, other);
        public override int GetHashCode() => GetHashCode(this);
        public override string ToString() => ToString(this);
    }

    public sealed class XorShift1024StarGenerator : IRandomGenerator<XorShift1024StarState>
    {
        /// <summary>
        /// The size of each "chunk" of bytes that can be generated at a time.
        /// </summary>
        public static readonly int ChunkSize = sizeof(ulong);

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        int IRandomGenerator<XorShift1024StarState>.ChunkSize => ChunkSize;

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        RandomnessKind IRandomGenerator<XorShift1024StarState>.RandomnessKind => RandomnessKind.PseudoRandom;

        /// <inheritdoc />
        public XorShift1024StarState FillBuffer(XorShift1024StarState state, byte[] buffer, int index, int count)
        {
            buffer.ValidateNotNull(nameof(buffer));
            index.ValidateInRange(nameof(index), 0, buffer.Length);

            if (buffer.Length - index < count)
            {
                throw new ArgumentException("Not enough room", nameof(buffer));
            }

            if (index % ChunkSize != 0)
            {
                throw new ArgumentException("Must be a multiple of ChunkSize.", nameof(index));
            }

            if (count % ChunkSize != 0)
            {
                throw new ArgumentException("Must be a multiple of ChunkSize.", nameof(count));
            }

            if (!state.IsValid)
            {
                throw new ArgumentException("State is not valid; use the parameterized constructor to initialize a new instance with the given seed values.", nameof(state));
            }

            FillBufferCore(ref state, buffer, index, count);
            return state;
        }

        private static unsafe void FillBufferCore(ref XorShift1024StarState state, byte[] buffer, int index, int count)
        {
            fixed (byte* fbuf = buffer)
            fixed (XorShift1024StarState* fState = &state)
            {
                // count has already been validated to be a multiple of ChunkSize,
                // and so has index, so we can do this fanciness without fear.
                ulong* pbuf = (ulong*)(fbuf + index);
                ulong* pend = pbuf + (count / ChunkSize);

                ulong* pState = (ulong*)fState;

                while (pbuf < pend)
                {
                    ulong s0 = *(pState + (state.p++));
                    state.p = state.p & 15;

                    ulong* pCurr = pState + state.p;
                    ulong s1 = *(pCurr);

                    s1 ^= s1 << 31;
                    s1 ^= s1 >> 11;
                    s0 ^= s0 >> 30;
                    *pCurr = s0 ^ s1;
                    *(pbuf++) = unchecked(1181783497276652981 * *(pCurr));
                }
            }
        }
    }
}
