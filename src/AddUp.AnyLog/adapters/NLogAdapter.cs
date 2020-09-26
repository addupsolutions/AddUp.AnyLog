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
            private readonly Action<object, object, Exception, string, object[]> logExceptionAndMessage;

            public InnerLogger(Type loggerType, Type logLevelType)
            {
                // logger, level, exception, message, args
                var logExceptionAndMessageMethodInfo = loggerType.GetMethod("Log", new[]
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

                var logExceptionAndMessageCall = Expression.Call(
                    loggerCast, logExceptionAndMessageMethodInfo, levelCast, exceptionParameter, messageParameter, argsParameter);

                logExceptionAndMessage = Expression.Lambda<Action<object, object, Exception, string, object[]>>(
                    logExceptionAndMessageCall, loggerParameter, levelParameter, exceptionParameter, messageParameter, argsParameter)
                    .Compile();
            }

            public void Log(object logger, object level, string message, Exception exception) =>
                logExceptionAndMessage(logger, level, exception, message, null);
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

        public void Log(string loggerName, LogLevel level, string message, Exception exception)
        {
            var namedLogger = namedLoggers.GetOrAdd(loggerName, getLoggerByName);
            var namedLoggerType = namedLogger.GetType();
            var innerLogger = innerLoggers.GetOrAdd(namedLoggerType, t => new InnerLogger(t, logLevelType));
            innerLogger.Log(namedLogger, logLevels[level], message, exception);
        }
    }
}
