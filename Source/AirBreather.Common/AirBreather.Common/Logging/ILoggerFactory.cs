namespace AirBreather.Common.Logging
{
    public interface ILoggerFactory
    {
        ILogger Create(string category);
    }
}

