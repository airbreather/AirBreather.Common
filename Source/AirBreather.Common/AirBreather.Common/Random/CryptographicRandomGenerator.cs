using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

using AirBreather.Common.Utilities;

namespace AirBreather.Common.Random
{
    // An IRandomGeneratorState type that carries no data.
    public struct EmptyRandomState : IEquatable<EmptyRandomState>, IRandomGeneratorState
    {
        public bool IsValid => true;
        public static bool operator ==(EmptyRandomState first, EmptyRandomState second) => true;
        public static bool operator !=(EmptyRandomState first, EmptyRandomState second) => false;
        public override bool Equals(object obj) => obj is EmptyRandomState;
        public bool Equals(EmptyRandomState other) => true;
        public override int GetHashCode() => 0;
        public override string ToString() => ToStringUtility.Begin(this).End();
    }

    // RNGCryptoServiceProvider claims to be thread-safe and carries no state.
    // Thus, we use EmptyRandomState as a placeholder.
    public sealed class CryptographicRandomGenerator : IRandomGenerator<EmptyRandomState>
    {
        /// <summary>
        /// The size of each "chunk" of bytes that can be generated at a time.
        /// </summary>
        public static readonly int ChunkSize = 1;

        private static readonly Lazy<RNGCryptoServiceProvider> SingletonProvider = new Lazy<RNGCryptoServiceProvider>();

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        int IRandomGenerator<EmptyRandomState>.ChunkSize => ChunkSize;

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        RandomnessKind IRandomGenerator<EmptyRandomState>.RandomnessKind => RandomnessKind.Random;

        /// <inheritdoc />
        public EmptyRandomState FillBuffer(EmptyRandomState state, byte[] buffer, int index, int count)
        {
            this.FillBuffer(buffer, index, count);
            return state;
        }

        public void FillBuffer(byte[] buffer, int index, int count)
        {
            buffer.ValidateNotNull(nameof(buffer));
            index.ValidateInRange(nameof(index), 0, buffer.Length);

            if (buffer.Length - index < count)
            {
                throw new ArgumentException("Not enough room", nameof(buffer));
            }

            if (index == 0 && count == buffer.Length)
            {
                // In this extremely common case, we can avoid an allocation and byte copy.
                SingletonProvider.Value.GetBytes(buffer);
            }
            else
            {
                // Otherwise...
                byte[] tempBuffer = new byte[count];
                SingletonProvider.Value.GetBytes(tempBuffer);
                Buffer.BlockCopy(tempBuffer, 0, buffer, index, count);
            }
        }

        // Offer quick-and-easy ways of generating random value types, because
        // this is often going to be used just to get a random seed for a PRNG.
        private readonly byte[] valueTypeBuffer = new byte[8];
        private readonly object valueTypeSyncLock = new object();

        public byte NextByte() => this.NextT((arr, idx) => arr[idx]);
        public sbyte NextSByte() => this.NextT((arr, idx) => unchecked((sbyte)arr[idx]));
        public short NextInt16() => this.NextT(BitConverter.ToInt16);
        public int NextInt32() => this.NextT(BitConverter.ToInt32);
        public long NextInt64() => this.NextT(BitConverter.ToInt64);
        public ushort NextUInt16() => this.NextT(BitConverter.ToUInt16);
        public uint NextUInt32() => this.NextT(BitConverter.ToUInt32);
        public ulong NextUInt64() => this.NextT(BitConverter.ToUInt64);

        public double NextDouble() => this.NextUInt64() / (double)ulong.MaxValue;
        public float NextSingle() => (float)this.NextDouble();

        private T NextT<T>(Func<byte[], int, T> afterGen)
        {
            lock (this.valueTypeSyncLock)
            {
                SingletonProvider.Value.GetBytes(this.valueTypeBuffer);
                return afterGen(this.valueTypeBuffer, 0);
            }
        }
    }
}

