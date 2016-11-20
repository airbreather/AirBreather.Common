using System;

namespace AirBreather.Logging
{
    public sealed class DumbLoggerFactory : ILoggerFactory
    {
        public ILogger Create(string category) => new DumbLogger(category);

        private sealed class DumbLogger : ILogger
        {
            private readonly string category;

            internal DumbLogger(string category) => this.category = category;

            public void Verbose(IFormatProvider formatProvider, string message, params object[] args) => Console.WriteLine(String.Format(formatProvider, "[Verbose] [" + this.category + "] " + message, args));
            public void Info(IFormatProvider formatProvider, string message, params object[] args) => Console.WriteLine(String.Format(formatProvider, "[Info] [" + this.category + "] " + message, args));
            public void Warn(IFormatProvider formatProvider, string message, params object[] args) => Console.WriteLine(String.Format(formatProvider, "[Warn] [" + this.category + "] " + message, args));
            public void Error(IFormatProvider formatProvider, string message, params object[] args) => Console.WriteLine(String.Format(formatProvider, "[Error] [" + this.category + "] " + message, args));
            public void Critical(IFormatProvider formatProvider, string message, params object[] args) => Console.WriteLine(String.Format(formatProvider, "[Critical] [" + this.category + "] " + message, args));
            public Exception Exception(Exception exception)
            {
                Console.WriteLine("[Exception] [" + this.category + "] " + exception);
                return exception;
            }
        }
    }
}

