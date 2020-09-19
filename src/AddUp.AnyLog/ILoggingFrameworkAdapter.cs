using System;

namespace AddUp.AnyLog
{
    internal interface ILoggingFrameworkAdapter
    {
        LoggingFrameworkDescriptor Descriptor { get; }
        void Log(string loggerName, LogLevel level, string message, Exception exception);
    }
}
