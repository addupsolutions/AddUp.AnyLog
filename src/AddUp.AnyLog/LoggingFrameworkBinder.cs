using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace AddUp.AnyLog
{
    [ExcludeFromCodeCoverage]
    internal sealed class LoggingFrameworkBinder
    {
        private readonly LoggingFrameworkDetector detector = new LoggingFrameworkDetector();

        public void Initialize()
        {
            detector.FrameworkDetected += OnFrameworkDetected;
            detector.Initialize();
        }

        private void OnFrameworkDetected(object sender, (LoggingFrameworkDescriptor descriptor, Assembly assy) e)
        {
            // Only change the current logger implementation if the detected one is "better"
            var current = LogManager.CurrentAdapter;
            if (e.descriptor.Preference <= current.Descriptor.Preference)
            {
                LogManager.Log.Trace($"{e.descriptor.Framework} Logging Framework was detected, but current framework ({current.Descriptor.Framework}) is preferred");
                return;
            }

            Exception error = null;
            ILoggingFrameworkAdapter adapter = null;
            try
            {
                adapter = e.descriptor.BuildAdapter(e.assy);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (adapter != null)
            {
                LogManager.SetAdapter(adapter);
                LogManager.Log.Trace($"Bound to {e.descriptor.Framework} Logging Framework");
            }
            else
            {
                var message = $"Could not create an instance of the adapter for {e.descriptor.Framework} Logging Framework";
                if (error != null)
                    LogManager.Log.Error(message + $": {error.Message}", error);
                else
                    LogManager.Log.Error(message);
            }
        }
    }
}
