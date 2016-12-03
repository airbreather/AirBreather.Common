using System;
using System.Globalization;

namespace AirBreather.Logging
{
    public static class Extensions
    {
        public static void Verbose(this ILogger logger, string message, params object[] args) => logger.ValidateNotNull(nameof(logger)).Verbose(CultureInfo.InvariantCulture, message, args);
        public static void Info(this ILogger logger, string message, params object[] args) => logger.ValidateNotNull(nameof(logger)).Info(CultureInfo.InvariantCulture, message, args);
        public static void Warn(this ILogger logger, string message, params object[] args) => logger.ValidateNotNull(nameof(logger)).Warn(CultureInfo.InvariantCulture, message, args);
        public static void Error(this ILogger logger, string message, params object[] args) => logger.ValidateNotNull(nameof(logger)).Error(CultureInfo.InvariantCulture, message, args);
        public static void Critical(this ILogger logger, string message, params object[] args) => logger.ValidateNotNull(nameof(logger)).Critical(CultureInfo.InvariantCulture, message, args);
        public static Exception Log<T>(this Exception exception, T exemplar) => Logging.Log.For(exemplar).Exception(exception);
        public static Exception Log(this Exception exception, Type type) => Logging.Log.For(type).Exception(exception);
    }
}
