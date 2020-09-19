using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace AddUp.AnyLog
{
    [SuppressMessage("Critical Code Smell", "S2696:Instance members should not write to \"static\" fields", Justification = "Cache usage")]
    internal sealed class CommonLoggingAdapter : ILoggingFrameworkAdapter
    {
        // Reflection cached data
        
        private static MethodInfo getLoggerMethodInfo;
        private static ConcurrentDictionary<string, object> loggers;
        private static ConcurrentDictionary<(string name, LogLevel level), MethodInfo> logMethodInfos;

        private readonly Assembly assy;

        public CommonLoggingAdapter(LoggingFrameworkDescriptor descriptor, Assembly assembly)
        {
            Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            assy = assembly ?? throw new ArgumentNullException(nameof(assembly));
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
            var manager = assy.DefinedTypes.Single(ti => ti.FullName == "Common.Logging.LogManager");
            getLoggerMethodInfo = manager.DeclaredMethods.Single(
                mi => mi.IsStatic && mi.Name == "GetLogger" && mi.GetParameters().Length == 1 && mi.GetParameters()[0].ParameterType == typeof(string));

            logMethodInfos = new ConcurrentDictionary<(string name, LogLevel level), MethodInfo>();
            loggers = new ConcurrentDictionary<string, object>();
        }        
    }    
}
