namespace AirBreather.Csv
{
    /// <summary>
    /// Represents the method that will handle the <see cref="Rfc4180CsvReader.FieldProcessed"/>
    /// event raised when the reader finishes reading a field.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">A <see cref="FieldProcessedEventArgs"/> that contains the event data.</param>
    public delegate void FieldProcessedEventHandler(object sender, FieldProcessedEventArgs args);
}
