using System;

namespace AddUp.AnyLog
{
    internal interface ILog
    {
        bool IsFatalEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsDebugEnabled { get; }
        bool IsTraceEnabled { get; }

        bool IsEnabled(LogLevel level);
        void Log(LogLevel level, string message, Exception exception);
    }
}
