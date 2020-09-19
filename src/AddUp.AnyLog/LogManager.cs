using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AddUp.AnyLog
{
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

            public override int GetHashCode()
            {
                var hashCode = -243844509;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Type);
                return hashCode;
            }
        }

        private sealed class LogImplementation : ILog
        {
            public LogImplementation(ILoggingFrameworkAdapter adapter, string name)
            {
                Adapter = adapter;
                Name = name ?? "";
            }

            private ILoggingFrameworkAdapter Adapter { get; }
            private string Name { get; }

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

        public static ILog GetLogger<T>() => GetLogger(typeof(T));
        public static ILog GetLogger(Type owningType) => GetLogger(owningType?.ToString() ?? "");
        public static ILog GetLogger(string name)
        {
            var adapter = CurrentAdapter;
            var key = new LogKey(name, adapter.GetType());
            return loggers.GetOrAdd(key, k => new LogImplementation(adapter, k.Name));
        }
    }    
}
