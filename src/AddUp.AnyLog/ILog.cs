using System;

namespace AddUp.AnyLog
{
    internal interface ILog
    {
        void Log(LogLevel level, string message, Exception exception);
    }
}
