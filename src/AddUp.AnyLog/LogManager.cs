using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace AddUp.AnyLog
{
    [ExcludeFromCodeCoverage]
    internal static class LogManager
    {
        private struct LogKey : IEquatable<LogKey>
        {
            public LogKey(string name, Type type)
            {
                Name = name ?? "";
                Type = (type ?? typeof(object)).FullName;
            }

            public string Name { get; }
            public string Type { get; }

            public override bool Equals(object obj) => obj is LogKey key && Equals(key);
            public bool Equals(LogKey other) => Name == other.Name && Type == other.Type;

            public override int GetHashCode() => (Name + Type).GetHashCode();

            public static bool operator ==(LogKey left, LogKey right) => left.Equals(right);
            public static bool operator !=(LogKey left, LogKey right) => !(left == right);
        }

        private sealed class LogImplementation : ILog
        {
            private readonly ConcurrentDictionary<LogLevel, bool> logLevelEnabledStates = new ConcurrentDictionary<LogLevel, bool>();

            public LogImplementation(ILoggingFrameworkAdapter adapter, string name, bool cacheLogLevelEnabledStates)
            {
                CacheLogLevelEnabledStates = cacheLogLevelEnabledStates;
                Adapter = adapter;
                Name = name ?? "";
            }

            private bool CacheLogLevelEnabledStates { get; }
            private ILoggingFrameworkAdapter Adapter { get; }
            private string Name { get; }

            public bool IsFatalEnabled => IsEnabled(LogLevel.Fatal);
            public bool IsErrorEnabled => IsEnabled(LogLevel.Error);
            public bool IsWarnEnabled => IsEnabled(LogLevel.Warn);
            public bool IsInfoEnabled => IsEnabled(LogLevel.Info);
            public bool IsDebugEnabled => IsEnabled(LogLevel.Debug);
            public bool IsTraceEnabled => IsEnabled(LogLevel.Trace);

            public bool IsEnabled(LogLevel level) => CacheLogLevelEnabledStates
                ? logLevelEnabledStates.GetOrAdd(level, l => Adapter.IsEnabled(Name, l))
                : Adapter.IsEnabled(Name, level);

            public void Log(LogLevel level, string message, Exception exception) => Adapter.Log(Name, level, message, exception);
        }

        private static readonly ConcurrentDictionary<LogKey, ILog> loggers = new ConcurrentDictionary<LogKey, ILog>();
        private static readonly LoggingFrameworkBinder binder = new LoggingFrameworkBinder();
        private static readonly ILoggingFrameworkAdapter defaultAdapter = LoggingFrameworkRegistry.DefaultAdapter;
        private static ILoggingFrameworkAdapter currentAdapter;

        static LogManager() =>  binder.Initialize();

        public static ILoggingFrameworkAdapter CurrentAdapter => currentAdapter ?? defaultAdapter;

        public static void SetAdapter(ILoggingFrameworkAdapter adapter) => currentAdapter = adapter;

        public static ILog Log => GetLogger("AnyLog");

        public static ILog GetLogger<T>(bool cacheLogLevelEnabledStates = true) => GetLogger(typeof(T), cacheLogLevelEnabledStates);
        public static ILog GetLogger(Type owningType, bool cacheLogLevelEnabledStates = true) => GetLogger(owningType?.ToString() ?? "", cacheLogLevelEnabledStates);
        public static ILog GetLogger(string name, bool cacheLogLevelEnabledStates = true)
        {
            var adapter = CurrentAdapter;
            var key = new LogKey(name, adapter.GetType());
            return loggers.GetOrAdd(key, k => new LogImplementation(adapter, k.Name, cacheLogLevelEnabledStates));
        }
    }    
}
