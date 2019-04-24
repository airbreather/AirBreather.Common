using System;

namespace AirBreather.Csv
{
    /// <summary>
    /// Provides data for the <see cref="Rfc4180CsvReader.FieldProcessed"/> event.
    /// </summary>
    /// <remarks>
    /// A <see cref="Rfc4180CsvReader.FieldProcessed"/> event is raised when the reader reaches the
    /// end of the UTF-8 encoded data for the next field in a RFC 4180 CSV stream.
    /// <see cref="Utf8FieldData"/> provides a reference to the encoded data.
    /// </remarks>
    public readonly ref struct FieldProcessedEventArgs
    {
        internal FieldProcessedEventArgs(ReadOnlySpan<byte> utf8FieldData) =>
            this.Utf8FieldData = utf8FieldData;

        /// <summary>
        /// Gets a <see cref="ReadOnlySpan{T}"/> pointing to the actual encoded field data, exactly
        /// as it was read from the user.
        /// </summary>
        /// <remarks>
        /// <para>
        /// An <see cref="ReadOnlySpan{T}.IsEmpty">empty</see> span is possible and indicates that
        /// a comma was immediately followed by another comma or line terminator.
        /// </para>
        /// <para>
        /// The reader does not guarantee that the bytes here represent a valid UTF-8 sequence.
        /// Rather, this span contains no more than the exact sequence of bytes between delimiters.
        /// Consumers are responsible for performing (or punting) such validation, depending on what
        /// they intend to actually do with the data.
        /// </para>
        /// </remarks>
        public ReadOnlySpan<byte> Utf8FieldData { get; }
    }
}
