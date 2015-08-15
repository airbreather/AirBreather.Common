using System;

namespace AirBreather.Common.Logging
{
    public interface ILogger
    {
        void Verbose(IFormatProvider formatProvider, string message, params object[] args);
        void Info(IFormatProvider formatProvider, string message, params object[] args);
        void Warn(IFormatProvider formatProvider, string message, params object[] args);
        void Error(IFormatProvider formatProvider, string message, params object[] args);
        void Critical(IFormatProvider formatProvider, string message, params object[] args);
        Exception Exception(Exception exception);
    }
}
