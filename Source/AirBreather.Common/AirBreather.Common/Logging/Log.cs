using System;

namespace AirBreather.Common.Logging
{
    public static class Log
    {
        public static ILoggerFactory Factory { get; set; } = new DumbLoggerFactory();

        public static ILogger For(Type type) => Factory.Create(type.Name);
        public static ILogger For<T>(T exemplar) => For(typeof(T));
    }
}
