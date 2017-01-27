using System;

namespace AirBreather.Random
{
    /// <summary>
    /// A generator of pseudorandom data.  Rather than maintaining its own state, the state is
    /// maintained outside of the generators.  This allows a single generator to be used from
    /// multiple threads at once, and it gives callers more freedom for how to manage state than
    /// more common "the generator maintains its own state" implementations.
    /// </summary>
    /// <typeparam name="TState">
    /// The type of <see cref="IRandomGeneratorState"/> that stores the state.
    /// </typeparam>
    /// <remarks>
    /// This is a bit of a "low-level" interface.  Callers must request chunk-sized blocks, mainly
    /// because the implementations tend to be well-defined algorithms that should exactly match
    /// counterparts in other languages which return one "chunk" (i.e., 64- or 32-bit integer) per
    /// method call.  Callers must also request chunk-aligned blocks, because the implementations
    /// tend to use unsafe code to eliminate what would otherwise involve extra allocations.  So,
    /// supporting partial or unaligned chunks gets complicated, especially if we want this to be
    /// fast and correct.  Furthermore, very few meaningful applications actually *need* streams of
    /// pseudorandom bytes themselves, but rather streams of higher-level pseudorandom concepts like
    /// "integer in range" or "random HLS / HSV color" that can be *produced* by reinterpreting a
    /// stream of bytes a particular way.  Those services can build off of this kind of provider.
    /// For these reasons, the "weirdness" of this interface should be isolated to providers of
    /// intermediate pseudorandom services rather than applications themselves.
    /// </remarks>
    public interface IRandomGenerator<TState> where TState: IRandomGeneratorState
    {
        /// <summary>
        /// Gets the size of each "chunk" of bytes that can be generated at a time.
        /// This value must remain constant throughout the lifetime of an instance.
        /// </summary>
        /// <remarks>
        /// Implementations of <see cref="FillBuffer"/> may reject spans with lengths that are not
        /// multiples of this value.
        /// </remarks>
        int ChunkSize { get; }

        /// <summary>
        /// Fills the specified range of the specified buffer with pseudorandom data.
        /// </summary>
        /// <param name="state">
        /// The <typeparamref name="TState"/> value that encapsulates the state of the generator.
        /// See remarks for expectations and guarantees.
        /// </param>
        /// <param name="buffer">
        /// A buffer that this method should populate with pseudorandom bytes.
        /// </param>
        /// <returns>
        /// A <typeparamref name="TState"/> value to use for subsequent calls, if needed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> is <see cref="Span{T}.Empty"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="state"/> is not <see cref="IRandomGeneratorState.IsValid">valid</see>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Results for equal <paramref name="state"/> values will always generate the same sequence
        /// of random bytes, and the return value after generating a given number of bytes will
        /// always be the same, no matter how many calls it took to generate that.  For example,
        /// generating 16 bytes, then passing the resulting state into another call that generates 8
        /// bytes will return the same state as requesting 24 bytes at once for the original state.
        /// </para>
        /// <para>
        /// Implementations are not <b>required</b> to throw <see cref="ArgumentException"/> when
        /// <paramref name="state"/> is not <see cref="IRandomGeneratorState.IsValid">valid</see>,
        /// but callers should still avoid calling this with such a state.
        /// </para>
        /// </remarks>
        TState FillBuffer(TState state, Span<byte> buffer);
    }
}
