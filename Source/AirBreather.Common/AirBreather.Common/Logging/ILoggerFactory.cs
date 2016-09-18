namespace AirBreather.Logging
{
    public interface ILoggerFactory
    {
        ILogger Create(string category);
    }
}

