using System;

namespace AirBreather.Common.Logging
{
    public static class Log
    {
        private static ILoggerFactory factory = new DumbLoggerFactory();
        public static ILoggerFactory Factory
        {
            get { return factory; }
            set { factory = value; }
        }

        public static ILogger For(Type type) => Factory.Create(type.Name);
        public static ILogger For<T>(T exemplar) => For(typeof(T));
    }
}
