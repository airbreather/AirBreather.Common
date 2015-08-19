// Ported from mt19937-64.c, which contains this copyright header:
/*
   Copyright (C) 2004, Makoto Matsumoto and Takuji Nishimura,
   All rights reserved.                          

   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:

     1. Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.

     2. Redistributions in binary form must reproduce the above copyright
        notice, this list of conditions and the following disclaimer in the
        documentation and/or other materials provided with the distribution.

     3. The names of its contributors may not be used to endorse or promote 
        products derived from this software without specific prior written 
        permission.

   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
   A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
   LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
   NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
   SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Diagnostics.CodeAnalysis;

using AirBreather.Common.Utilities;

namespace AirBreather.Common.Random
{
    public struct MT19937_64State : IEquatable<MT19937_64State>, IRandomGeneratorState
    {
        internal ulong s0; internal ulong s1; internal ulong s2; internal ulong s3; internal ulong s4; internal ulong s5; internal ulong s6; internal ulong s7; internal ulong s8; internal ulong s9; internal ulong s10; internal ulong s11; internal ulong s12; internal ulong s13; internal ulong s14; internal ulong s15; internal ulong s16; internal ulong s17; internal ulong s18; internal ulong s19; internal ulong s20; internal ulong s21; internal ulong s22; internal ulong s23; internal ulong s24; internal ulong s25; internal ulong s26; internal ulong s27; internal ulong s28; internal ulong s29; internal ulong s30; internal ulong s31; internal ulong s32; internal ulong s33; internal ulong s34; internal ulong s35; internal ulong s36; internal ulong s37; internal ulong s38; internal ulong s39; internal ulong s40; internal ulong s41; internal ulong s42; internal ulong s43; internal ulong s44; internal ulong s45; internal ulong s46; internal ulong s47; internal ulong s48; internal ulong s49; internal ulong s50; internal ulong s51; internal ulong s52; internal ulong s53; internal ulong s54; internal ulong s55; internal ulong s56; internal ulong s57; internal ulong s58; internal ulong s59; internal ulong s60; internal ulong s61; internal ulong s62; internal ulong s63; internal ulong s64; internal ulong s65; internal ulong s66; internal ulong s67; internal ulong s68; internal ulong s69; internal ulong s70; internal ulong s71; internal ulong s72; internal ulong s73; internal ulong s74; internal ulong s75; internal ulong s76; internal ulong s77; internal ulong s78; internal ulong s79; internal ulong s80; internal ulong s81; internal ulong s82; internal ulong s83; internal ulong s84; internal ulong s85; internal ulong s86; internal ulong s87; internal ulong s88; internal ulong s89; internal ulong s90; internal ulong s91; internal ulong s92; internal ulong s93; internal ulong s94; internal ulong s95; internal ulong s96; internal ulong s97; internal ulong s98; internal ulong s99; internal ulong s100; internal ulong s101; internal ulong s102; internal ulong s103; internal ulong s104; internal ulong s105; internal ulong s106; internal ulong s107; internal ulong s108; internal ulong s109; internal ulong s110; internal ulong s111; internal ulong s112; internal ulong s113; internal ulong s114; internal ulong s115; internal ulong s116; internal ulong s117; internal ulong s118; internal ulong s119; internal ulong s120; internal ulong s121; internal ulong s122; internal ulong s123; internal ulong s124; internal ulong s125; internal ulong s126; internal ulong s127; internal ulong s128; internal ulong s129; internal ulong s130; internal ulong s131; internal ulong s132; internal ulong s133; internal ulong s134; internal ulong s135; internal ulong s136; internal ulong s137; internal ulong s138; internal ulong s139; internal ulong s140; internal ulong s141; internal ulong s142; internal ulong s143; internal ulong s144; internal ulong s145; internal ulong s146; internal ulong s147; internal ulong s148; internal ulong s149; internal ulong s150; internal ulong s151; internal ulong s152; internal ulong s153; internal ulong s154; internal ulong s155; internal ulong s156; internal ulong s157; internal ulong s158; internal ulong s159; internal ulong s160; internal ulong s161; internal ulong s162; internal ulong s163; internal ulong s164; internal ulong s165; internal ulong s166; internal ulong s167; internal ulong s168; internal ulong s169; internal ulong s170; internal ulong s171; internal ulong s172; internal ulong s173; internal ulong s174; internal ulong s175; internal ulong s176; internal ulong s177; internal ulong s178; internal ulong s179; internal ulong s180; internal ulong s181; internal ulong s182; internal ulong s183; internal ulong s184; internal ulong s185; internal ulong s186; internal ulong s187; internal ulong s188; internal ulong s189; internal ulong s190; internal ulong s191; internal ulong s192; internal ulong s193; internal ulong s194; internal ulong s195; internal ulong s196; internal ulong s197; internal ulong s198; internal ulong s199; internal ulong s200; internal ulong s201; internal ulong s202; internal ulong s203; internal ulong s204; internal ulong s205; internal ulong s206; internal ulong s207; internal ulong s208; internal ulong s209; internal ulong s210; internal ulong s211; internal ulong s212; internal ulong s213; internal ulong s214; internal ulong s215; internal ulong s216; internal ulong s217; internal ulong s218; internal ulong s219; internal ulong s220; internal ulong s221; internal ulong s222; internal ulong s223; internal ulong s224; internal ulong s225; internal ulong s226; internal ulong s227; internal ulong s228; internal ulong s229; internal ulong s230; internal ulong s231; internal ulong s232; internal ulong s233; internal ulong s234; internal ulong s235; internal ulong s236; internal ulong s237; internal ulong s238; internal ulong s239; internal ulong s240; internal ulong s241; internal ulong s242; internal ulong s243; internal ulong s244; internal ulong s245; internal ulong s246; internal ulong s247; internal ulong s248; internal ulong s249; internal ulong s250; internal ulong s251; internal ulong s252; internal ulong s253; internal ulong s254; internal ulong s255; internal ulong s256; internal ulong s257; internal ulong s258; internal ulong s259; internal ulong s260; internal ulong s261; internal ulong s262; internal ulong s263; internal ulong s264; internal ulong s265; internal ulong s266; internal ulong s267; internal ulong s268; internal ulong s269; internal ulong s270; internal ulong s271; internal ulong s272; internal ulong s273; internal ulong s274; internal ulong s275; internal ulong s276; internal ulong s277; internal ulong s278; internal ulong s279; internal ulong s280; internal ulong s281; internal ulong s282; internal ulong s283; internal ulong s284; internal ulong s285; internal ulong s286; internal ulong s287; internal ulong s288; internal ulong s289; internal ulong s290; internal ulong s291; internal ulong s292; internal ulong s293; internal ulong s294; internal ulong s295; internal ulong s296; internal ulong s297; internal ulong s298; internal ulong s299; internal ulong s300; internal ulong s301; internal ulong s302; internal ulong s303; internal ulong s304; internal ulong s305; internal ulong s306; internal ulong s307; internal ulong s308; internal ulong s309; internal ulong s310; internal ulong s311;
        internal int idx;

        public unsafe MT19937_64State(ulong seed)
        {
            fixed (MT19937_64State* fThis = &this)
            {
                ulong* p = (ulong*)(fThis);
                *p++ = seed;
                for (int i = 1; i < 312; i++)
                {
                    ulong prev = *(p - 1);
                    *(p++) = unchecked((6364136223846793005 * (prev ^ (prev >> 62))) + (ulong)i);
                }
            }

            this.idx = 312;
        }

        // TODO: our cousins in other languages allow seeding by multiple input values... support that too.

        public unsafe MT19937_64State(MT19937_64State copyFrom)
        {
            fixed (MT19937_64State* fThis = &this)
            {
                ulong* s = (ulong*)fThis;
                ulong* t = (ulong*)&copyFrom;
                for (int i = 0; i < 312; i++)
                {
                    *(s++) = *(t++);
                }
            }

            this.idx = copyFrom.idx;
        }

        private static bool StateIsValid(MT19937_64State state) => 0 <= state.idx && state.idx <= 312;
        public bool IsValid => StateIsValid(this);

        public static unsafe bool Equals(MT19937_64State first, MT19937_64State second)
        {
            if (first.idx != second.idx)
            {
                return false;
            }

            ulong* s = (ulong*)&first;
            ulong* t = (ulong*)&second;
            for (int i = 0; i < 312; i++)
            {
                if (*(s++) != *(t++))
                {
                    return false;
                }
            }

            return true;
        }

        public static unsafe int GetHashCode(MT19937_64State state)
        {
            int hashCode = HashCodeUtility.Seed;

            hashCode = hashCode.HashWith(state.idx);
            ulong* p = (ulong*)&state;
            for (int i = 0; i < 312; i++)
            {
                hashCode = hashCode.HashWith(*(p++));
            }

            return hashCode;
        }

        public static string ToString(MT19937_64State state) => ToStringUtility.Begin(state).End();

        public static bool operator ==(MT19937_64State first, MT19937_64State second) => Equals(first, second);
        public static bool operator !=(MT19937_64State first, MT19937_64State second) => !Equals(first, second);
        public override bool Equals(object obj) => obj is MT19937_64State && Equals(this, (MT19937_64State)obj);
        public bool Equals(MT19937_64State other) => Equals(this, other);
        public override int GetHashCode() => GetHashCode(this);
        public override string ToString() => ToString(this);
    }

    public sealed class MT19937_64Generator : IRandomGenerator<MT19937_64State>
    {
        /// <summary>
        /// The size of each "chunk" of bytes that can be generated at a time.
        /// </summary>
        public static readonly int ChunkSize = sizeof(ulong);

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        int IRandomGenerator<MT19937_64State>.ChunkSize => ChunkSize;

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        RandomnessKind IRandomGenerator<MT19937_64State>.RandomnessKind => RandomnessKind.PseudoRandom;

        /// <inheritdoc />
        public MT19937_64State FillBuffer(MT19937_64State state, byte[] buffer, int index, int count)
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

        private static unsafe void FillBufferCore(ref MT19937_64State state, byte[] buffer, int index, int count)
        {
            fixed (MT19937_64State* fState = &state)
            fixed (byte* fbuf = buffer)
            {
                ulong* s = (ulong*)fState;

                // count has already been validated to be a multiple of ChunkSize,
                // and so has index, so we can do this fanciness without fear.
                ulong* pbuf = (ulong*)(fbuf + index);
                ulong* pend = pbuf + (count / ChunkSize);
                while (pbuf < pend)
                {
                    if (state.idx == 312)
                    {
                        Twist(s);
                        state.idx = 0;
                    }

                    ulong x = s[state.idx++];

                    x ^= (x >> 29) & 0x5555555555555555;
                    x ^= (x << 17) & 0x71D67FFFEDA60000;
                    x ^= (x << 37) & 0xFFF7EEE000000000;
                    x ^= (x >> 43);

                    *(pbuf++) = x;
                }
            }
        }

        private static unsafe void Twist(ulong* vals)
        {
            const ulong Upper33 = 0xFFFFFFFF80000000;
            const ulong Lower31 = 0x000000007FFFFFFF;

            for (int curr = 0; curr < 312; curr++)
            {
                int near = (curr + 1) % 312;
                int far = (curr + 156) % 312;

                ulong x = vals[curr] & Upper33;
                ulong y = vals[near] & Lower31;
                ulong z = vals[far] ^ ((x | y) >> 1);

                vals[curr] = z ^ ((y & 1) * 0xB5026F5AA96619E9);
            }
        }
    }
}
