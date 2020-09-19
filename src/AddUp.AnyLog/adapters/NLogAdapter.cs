using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace AddUp.AnyLog
{
    [SuppressMessage("Critical Code Smell", "S2696:Instance members should not write to \"static\" fields", Justification = "Cache usage")]
    internal sealed class NLogAdapter : ILoggingFrameworkAdapter
    {
        // Reflection cached data
        private static TypeInfo logLevelTypeInfo;
        private static MethodInfo getLoggerMethodInfo;
        private static IReadOnlyDictionary<LogLevel, object> logLevels;
        private static ConcurrentDictionary<string, object> loggers;
        private static ConcurrentDictionary<string, MethodInfo> logMethodInfos;
        private static ConcurrentDictionary<string, MethodInfo> logExceptionMethodInfos;

        private readonly Assembly assy;

        public NLogAdapter(LoggingFrameworkDescriptor descriptor, Assembly assembly)
        {
            Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            assy = assembly ?? throw new ArgumentNullException(nameof(assembly));
            InitializeReflectionCache();
        }

        public LoggingFrameworkDescriptor Descriptor { get; }

        public void Log(string loggerName, LogLevel level, string message, Exception exception)
        {
            var logger = loggers.GetOrAdd(loggerName, n => getLoggerMethodInfo.Invoke(null, new[] { n }));
            if (exception == null)
            {
                var logMethodInfo = GetLogMethodInfo(loggerName, logger.GetType());
                _ = logMethodInfo.Invoke(logger, new[] { logLevels[level], message });
            }
            else
            {
                var logMethodInfo = GetLogExceptionMethodInfo(loggerName, logger.GetType());
                _ = logMethodInfo.Invoke(logger, new[] { logLevels[level], message, exception });
            }
        }

        private MethodInfo GetLogMethodInfo(string loggerName, Type loggerType) =>
            logMethodInfos.GetOrAdd(loggerName, n => loggerType.GetMethod("Log", new[]
            {
                logLevelTypeInfo.AsType(), // level
                typeof(string) // message
            }));

        private MethodInfo GetLogExceptionMethodInfo(string loggerName, Type loggerType) =>
            logExceptionMethodInfos.GetOrAdd(loggerName, n => loggerType.GetMethod("Log", new[]
            {
                logLevelTypeInfo.AsType(), // level
                typeof(string), // message
                typeof(Exception), // exception
            }));

        private void InitializeReflectionCache()
        {
            // First, let's retrieve and cache a few things from Reflection.
            var manager = assy.DefinedTypes.Single(ti => ti.FullName == "NLog.LogManager");
            getLoggerMethodInfo = manager.DeclaredMethods.Single(
                mi => mi.IsStatic && mi.Name == "GetLogger" && mi.GetParameters().Length == 1 && mi.GetParameters()[0].ParameterType == typeof(string));

            logLevelTypeInfo = assy.DefinedTypes.Single(ti => ti.FullName == "NLog.LogLevel");
            logLevels = new Dictionary<LogLevel, object>
            {
                [LogLevel.Fatal] = logLevelTypeInfo.GetField("Fatal").GetValue(null),
                [LogLevel.Error] = logLevelTypeInfo.GetField("Error").GetValue(null),
                [LogLevel.Warn] = logLevelTypeInfo.GetField("Warn").GetValue(null),
                [LogLevel.Info] = logLevelTypeInfo.GetField("Info").GetValue(null),
                [LogLevel.Debug] = logLevelTypeInfo.GetField("Debug").GetValue(null),
                [LogLevel.Trace] = logLevelTypeInfo.GetField("Trace").GetValue(null)
            };

            // Depending on whether we wish to log an exception trace or not, the 'log' method is not the same
            logMethodInfos = new ConcurrentDictionary<string, MethodInfo>();
            logExceptionMethodInfos = new ConcurrentDictionary<string, MethodInfo>();

            loggers = new ConcurrentDictionary<string, object>();
        }        
    }
}
