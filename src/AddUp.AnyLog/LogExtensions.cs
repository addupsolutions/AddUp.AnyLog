using System;

namespace AddUp.AnyLog
{
    internal static class LogExtensions
    {
        public static void Log(this ILog log, LogLevel level, string message) => log.Log(level, message, null);
        public static void Log(this ILog log, LogLevel level, Exception exception) => log.Log(level, null, exception);

        public static void Fatal(this ILog log, string message) => log.Fatal(message, null);
        public static void Fatal(this ILog log, Exception exception) => log.Fatal(null, exception);
        public static void Fatal(this ILog log, string message, Exception exception) => log.Log(LogLevel.Fatal, message, exception);

        public static void Error(this ILog log, string message) => log.Error(message, null);
        public static void Error(this ILog log, Exception exception) => log.Error(null, exception);
        public static void Error(this ILog log, string message, Exception exception) => log.Log(LogLevel.Error, message, exception);

        public static void Warn(this ILog log, string message) => log.Warn(message, null);
        public static void Warn(this ILog log, Exception exception) => log.Warn(null, exception);
        public static void Warn(this ILog log, string message, Exception exception) => log.Log(LogLevel.Warn, message, exception);

        public static void Info(this ILog log, string message) => log.Info(message, null);
        public static void Info(this ILog log, Exception exception) => log.Info(null, exception);
        public static void Info(this ILog log, string message, Exception exception) => log.Log(LogLevel.Info, message, exception);

        public static void Debug(this ILog log, string message) => log.Debug(message, null);
        public static void Debug(this ILog log, Exception exception) => log.Debug(null, exception);
        public static void Debug(this ILog log, string message, Exception exception) => log.Log(LogLevel.Debug, message, exception);

        public static void Trace(this ILog log, string message) => log.Trace(message, null);
        public static void Trace(this ILog log, Exception exception) => log.Trace(null, exception);
        public static void Trace(this ILog log, string message, Exception exception) => log.Log(LogLevel.Trace, message, exception);
    }
}
