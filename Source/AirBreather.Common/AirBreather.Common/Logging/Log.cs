using System.Reflection;

namespace AirBreather.Logging
{
    public static class Log
    {
        public static ILoggerFactory Factory { get; set; } = new DumbLoggerFactory();

        public static ILogger For(MemberInfo type) => Factory.Create(type.Name);
        public static ILogger For<T>(T exemplar) => For(typeof(T));
    }
}
