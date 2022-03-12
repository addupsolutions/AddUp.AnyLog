using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AddUp.AnyLog
{
    [ExcludeFromCodeCoverage]
    internal sealed class NLogAdapter : ILoggingFrameworkAdapter
    {
        private sealed class InnerLogger
        {
            private readonly Func<object, object, bool> isLogLevelEnabled;
            private readonly Action<object, object, Exception, string, object[]> logExceptionAndMessage;

            public InnerLogger(Type loggerType, Type logLevelType)
            {
                isLogLevelEnabled = BuildIsLogLevelEnabledMethod(loggerType, logLevelType);
                logExceptionAndMessage = BuildLogExceptionAndMessageMethod(loggerType, logLevelType);
            }

            private Func<object, object, bool> BuildIsLogLevelEnabledMethod(Type loggerType, Type logLevelType)
            {
                var methodInfo = loggerType.GetMethod("IsEnabled", new[] { logLevelType });

                var loggerParameter = Expression.Parameter(typeof(object));
                var loggerCast = Expression.Convert(loggerParameter, loggerType);
                var levelParameter = Expression.Parameter(typeof(object));
                var levelCast = Expression.Convert(levelParameter, logLevelType);

                var call = Expression.Call(loggerCast, methodInfo, levelCast);

                return Expression.Lambda<Func<object, object, bool>>(call, loggerParameter, levelParameter)
                    .Compile();
            }

            private Action<object, object, Exception, string, object[]> BuildLogExceptionAndMessageMethod(Type loggerType, Type logLevelType)
            {
                var methodInfo = loggerType.GetMethod("Log", new[]
                {
                    logLevelType, // level
                    typeof(Exception), // exception
                    typeof(string), // message
                    typeof(object[]) // args
                });

                var loggerParameter = Expression.Parameter(typeof(object));
                var loggerCast = Expression.Convert(loggerParameter, loggerType);
                var levelParameter = Expression.Parameter(typeof(object));
                var levelCast = Expression.Convert(levelParameter, logLevelType);
                var exceptionParameter = Expression.Parameter(typeof(Exception));
                var messageParameter = Expression.Parameter(typeof(string));
                var argsParameter = Expression.Parameter(typeof(object[]));

                var call = Expression.Call(
                    loggerCast, methodInfo, levelCast, exceptionParameter, messageParameter, argsParameter);

                return Expression.Lambda<Action<object, object, Exception, string, object[]>>(
                    call, loggerParameter, levelParameter, exceptionParameter, messageParameter, argsParameter)
                    .Compile();
            }

            public bool IsEnabled(object nlogLogger, object logLevel) => isLogLevelEnabled(nlogLogger, logLevel);

            public void Log(object nlogLogger, object nlogLevel, string message, Exception exception) =>
                logExceptionAndMessage(nlogLogger, nlogLevel, exception, message, null);
        }

        // Reflection data
        private readonly Assembly assy;
        private readonly Func<string, object> getLoggerByName;
        private readonly IReadOnlyDictionary<LogLevel, object> logLevels;
        private readonly Type logLevelType;
        private readonly ConcurrentDictionary<string, object> namedLoggers;
        private readonly ConcurrentDictionary<Type, InnerLogger> innerLoggers;

        public NLogAdapter(LoggingFrameworkDescriptor descriptor, Assembly assembly)
        {
            Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            assy = assembly ?? throw new ArgumentNullException(nameof(assembly));

            var manager = assy.DefinedTypes.Single(ti => ti.FullName == "NLog.LogManager");
            var getLoggerMethodInfo = manager.DeclaredMethods.Single(
                mi => mi.IsStatic &&
                mi.Name == "GetLogger" &&
                mi.GetParameters().Length == 1 &&
                mi.GetParameters()[0].ParameterType == typeof(string));

            var parameter = Expression.Parameter(typeof(string), "name");
            var call = Expression.Call(null, getLoggerMethodInfo, parameter);
            getLoggerByName = Expression
                .Lambda<Func<string, object>>(call, parameter)
                .Compile();

            var logLevelTypeInfo = assy.DefinedTypes.Single(ti => ti.FullName == "NLog.LogLevel");
            logLevels = new Dictionary<LogLevel, object>
            {
                [LogLevel.Fatal] = logLevelTypeInfo.GetField("Fatal").GetValue(null),
                [LogLevel.Error] = logLevelTypeInfo.GetField("Error").GetValue(null),
                [LogLevel.Warn] = logLevelTypeInfo.GetField("Warn").GetValue(null),
                [LogLevel.Info] = logLevelTypeInfo.GetField("Info").GetValue(null),
                [LogLevel.Debug] = logLevelTypeInfo.GetField("Debug").GetValue(null),
                [LogLevel.Trace] = logLevelTypeInfo.GetField("Trace").GetValue(null)
            };

            logLevelType = logLevelTypeInfo.AsType();
            namedLoggers = new ConcurrentDictionary<string, object>();
            innerLoggers = new ConcurrentDictionary<Type, InnerLogger>();
        }

        public LoggingFrameworkDescriptor Descriptor { get; }

        public bool IsEnabled(string loggerName, LogLevel level) =>
            GetLogger(loggerName, out var nlogLogger).IsEnabled(nlogLogger, logLevels[level]);

        public void Log(string loggerName, LogLevel level, string message, Exception exception) =>
            GetLogger(loggerName, out var nlogLogger).Log(nlogLogger, logLevels[level], message, exception);

        private InnerLogger GetLogger(string loggerName, out object nlogLogger)
        {
            nlogLogger = namedLoggers.GetOrAdd(loggerName, getLoggerByName);
            return innerLoggers.GetOrAdd(nlogLogger.GetType(), t => new InnerLogger(t, logLevelType));
        }
    }
}
