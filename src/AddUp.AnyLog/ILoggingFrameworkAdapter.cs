using System;

namespace AddUp.AnyLog
{
    internal interface ILoggingFrameworkAdapter
    {
        LoggingFrameworkDescriptor Descriptor { get; }
        bool IsEnabled(string loggerName, LogLevel level);
        void Log(string loggerName, LogLevel level, string message, Exception exception);
    }
}
