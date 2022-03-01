using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace AddUp.AnyLog
{
    [ExcludeFromCodeCoverage]
    internal sealed class CommonLoggingFamilyAdapter : ILoggingFrameworkAdapter
    {
        // Reflection cached data
        
        private static MethodInfo getLoggerMethodInfo;
        private static ConcurrentDictionary<string, object> loggers;
        private static ConcurrentDictionary<(string name, LogLevel level), MethodInfo> logMethodInfos;

        private readonly bool isAddUpVariant;
        private readonly Assembly assy;

        public CommonLoggingFamilyAdapter(LoggingFrameworkDescriptor descriptor, Assembly assembly, bool addUpVariant)
        {
            Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            assy = assembly ?? throw new ArgumentNullException(nameof(assembly));
            isAddUpVariant = addUpVariant;
            InitializeReflectionCache();
        }

        public LoggingFrameworkDescriptor Descriptor { get; }

        public void Log(string loggerName, LogLevel level, string message, Exception exception)
        {
            var logger = loggers.GetOrAdd(loggerName, n => getLoggerMethodInfo.Invoke(null, new[] { n }));
            var logMethodInfo = logMethodInfos.GetOrAdd((loggerName, level), k =>
            {
                switch (k.level)
                {
                    case LogLevel.Fatal: return logger.GetType().GetMethod("Fatal", new[] { typeof(object), typeof(Exception) });
                    case LogLevel.Error: return logger.GetType().GetMethod("Error", new[] { typeof(object), typeof(Exception) });
                    case LogLevel.Warn: return logger.GetType().GetMethod("Warn", new[] { typeof(object), typeof(Exception) });
                    case LogLevel.Info: return logger.GetType().GetMethod("Info", new[] { typeof(object), typeof(Exception) });
                    case LogLevel.Debug: return logger.GetType().GetMethod("Debug", new[] { typeof(object), typeof(Exception) });
                    case LogLevel.Trace: return logger.GetType().GetMethod("Trace", new[] { typeof(object), typeof(Exception) });
                }

                return logger.GetType().GetMethod("Info", new[] { typeof(object), typeof(Exception) });
            });

            _ = logMethodInfo.Invoke(logger, new[] { (object)message, exception });
        }

        private void InitializeReflectionCache()
        {
            // First, let's retrieve and cache a few things from Reflection.
            var managerTypeName = isAddUpVariant ? "AddUp.CommonLogging.LogManager" : "Common.Logging.LogManager";
            var manager = assy.DefinedTypes.Single(ti => ti.FullName == managerTypeName);
            getLoggerMethodInfo = manager.DeclaredMethods.Single(
                mi => mi.IsStatic && mi.Name == "GetLogger" && mi.GetParameters().Length == 1 && mi.GetParameters()[0].ParameterType == typeof(string));

            logMethodInfos = new ConcurrentDictionary<(string name, LogLevel level), MethodInfo>();
            loggers = new ConcurrentDictionary<string, object>();
        }        
    }    
}
