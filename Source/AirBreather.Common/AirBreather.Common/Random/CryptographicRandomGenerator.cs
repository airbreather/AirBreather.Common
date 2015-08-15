using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

using AirBreather.Common.Utilities;

namespace AirBreather.Common.Random
{
    // An IRandomGeneratorState type that carries no data.
    public struct EmptyRandomState : IEquatable<EmptyRandomState>, IRandomGeneratorState
    {
        private static readonly string Repr = ToStringUtility.Begin(default(EmptyRandomState)).End();

        public bool IsValid => true;
        public static bool operator ==(EmptyRandomState first, EmptyRandomState second) => true;
        public static bool operator !=(EmptyRandomState first, EmptyRandomState second) => false;
        public override bool Equals(object obj) => obj is EmptyRandomState;
        public bool Equals(EmptyRandomState other) => true;
        public override int GetHashCode() => 0;
        public override string ToString() => Repr;
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

        public EmptyRandomState FillBuffer(EmptyRandomState state, byte[] buffer, int index, int count)
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

            return default(EmptyRandomState);
        }
    }
}

