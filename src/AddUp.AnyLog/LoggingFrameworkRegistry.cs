using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace AddUp.AnyLog
{
    internal enum LoggingFramework
    {
        Integrated,
        CommonLogging,
        NLog
    }

    [ExcludeFromCodeCoverage]
    internal class LoggingFrameworkDescriptor
    {
        public LoggingFrameworkDescriptor(LoggingFramework fx) => Framework = fx;

        public LoggingFramework Framework { get; }
        public string Name => Framework.ToString();
        public string TestTypeName { get; set; }
        public int Preference { get; set; }
        public Func<LoggingFrameworkDescriptor, Assembly, ILoggingFrameworkAdapter> AdapterBuilder { get; set; }

        public ILoggingFrameworkAdapter BuildAdapter(Assembly assy) => AdapterBuilder(this, assy);
    }

    internal static class LoggingFrameworkRegistry
    {
        private const int lowest = 0; // For DefaultAdapter
        private const int average = 10; // For Common.Logging or other wrappers
        private const int highest = 20; // For full-fledged frameworks

        private static readonly Dictionary<LoggingFramework, LoggingFrameworkDescriptor> descriptors = new Dictionary<LoggingFramework, LoggingFrameworkDescriptor>();

        static LoggingFrameworkRegistry()
        {
            // Special case: defaultDesriptor definition is a bit recursive...
            var defaultDesriptor = new LoggingFrameworkDescriptor(LoggingFramework.Integrated)
            {
                TestTypeName = "",
                Preference = lowest
            };

            DefaultAdapter = new DefaultAdapter(defaultDesriptor);
            defaultDesriptor.AdapterBuilder = (_, __) => DefaultAdapter;
            descriptors.Add(LoggingFramework.Integrated, defaultDesriptor);

            void add(LoggingFramework fx, string typeName, int preference, Func<LoggingFrameworkDescriptor, Assembly, ILoggingFrameworkAdapter> builder) =>
                descriptors.Add(fx, new LoggingFrameworkDescriptor(fx) { TestTypeName = typeName, Preference = preference, AdapterBuilder = builder });

            add(LoggingFramework.CommonLogging, "Common.Logging.LogManager", average, (d, assy) => new CommonLoggingAdapter(d, assy));
            add(LoggingFramework.NLog, "NLog.LogManager", highest, (d, assy) => new NLogAdapter(d, assy));
        }

        public static IEnumerable<LoggingFrameworkDescriptor> Descriptors => descriptors.Values;
        public static IEnumerable<LoggingFramework> Frameworks => descriptors.Keys;
        public static ILoggingFrameworkAdapter DefaultAdapter { get; }

        // Returns null if not found
        public static LoggingFrameworkDescriptor GetDescriptorFromTypeName(string candidate) =>
            Descriptors.FirstOrDefault(d => d.TestTypeName == candidate);
    }
}
