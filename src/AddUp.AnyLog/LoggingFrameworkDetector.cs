using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace AddUp.AnyLog
{
    [ExcludeFromCodeCoverage]
    internal sealed class LoggingFrameworkDetector
    {
        private readonly Dictionary<LoggingFramework, (LoggingFrameworkDescriptor descriptor, Assembly assy)> detectedFrameworks = 
            new Dictionary<LoggingFramework, (LoggingFrameworkDescriptor descriptor, Assembly assy)>();

        public event EventHandler<(LoggingFrameworkDescriptor descriptor, Assembly assy)> FrameworkDetected;

        public IReadOnlyDictionary<LoggingFramework, (LoggingFrameworkDescriptor descriptor, Assembly assy)> DetectedFrameworks => detectedFrameworks;

        public void Initialize()
        {
            PreventCrash();
            
            AppDomain.CurrentDomain.AssemblyLoad += (_, e) => ExamineAssembly(e.LoadedAssembly);

            // Force detection in already loaded assemblies
            foreach (var assy in AppDomain.CurrentDomain.GetAssemblies())
                ExamineAssembly(assy);
        }

        private void ExamineAssembly(Assembly assy)
        {
            var name = assy.GetName().Name;
            if (name == "mscorlib" || name == "System" || name.StartsWith("System.")) return; // Might need to tweak this if we are to support Microsoft logging

            var foundDescriptors = assy.GetTypes().Select(type => LoggingFrameworkRegistry.GetDescriptorFromTypeName(type.FullName)).Where(d => d != null);
            foreach (var descriptor in foundDescriptors)
            {
                var fx = descriptor.Framework;
                if (!DetectedFrameworks.ContainsKey(fx))
                {
                    detectedFrameworks.Add(fx, (descriptor, assy));
                    FrameworkDetected?.Invoke(this, (descriptor, assy));
                }
            }
        }

        private static void PreventCrash() // TODO: make sure this is not optimized away...
        {
            // by executing a few seemingly useless instructions, we force loading of a minimalist set of .NET System assemblies 
            // before we examine future assembly loads
            // In particular, we'd like mscorlib, System and System.Core to be available...
            // See https://stackoverflow.com/a/48885319/107552

            // System.Core will have mscorlib and System loaded if they were not already (or equivalent assemblies in .NET Core)
            var data = new[] { "foo", "bar", "baz" };
            _ = data.Where(x => x == "foo");
        }
    }
}
