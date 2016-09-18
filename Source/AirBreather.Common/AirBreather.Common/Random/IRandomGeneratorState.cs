namespace AirBreather.Random
{
    public interface IRandomGeneratorState
    {
        /// <summary>
        /// Gets a value indicating whether or not this instance is valid.
        /// </summary>
        bool IsValid { get; }
    }
}