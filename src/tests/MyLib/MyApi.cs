using System;
using AddUp.AnyLog;

namespace MyLib
{
    public sealed class MyApi
    {
        private static readonly ILog log = LogManager.GetLogger<MyApi>();

        public void DoSomething(string input, bool fail)
        {
            try
            {
                var text = PrivateImplementation(input, fail);

                // Test all log levels
                var f = log.IsFatalEnabled ? 1 : 0;
                var e = log.IsErrorEnabled ? 1 : 0;
                var w = log.IsWarnEnabled ? 1 : 0;
                var i = log.IsInfoEnabled ? 1 : 0;
                var d = log.IsDebugEnabled ? 1 : 0;
                var t = log.IsTraceEnabled ? 1 : 0;

                log.Info($"Enabled States: {f}{e}{w}{i}{d}{t}");
                log.Fatal($"Input was: {text} - Enabled: {log.IsFatalEnabled}");
                log.Error($"Input was: {text} - Enabled: {log.IsErrorEnabled}");
                log.Warn($"Input was: {text} - Enabled: {log.IsWarnEnabled}");
                log.Info($"Input was: {text} - Enabled: {log.IsInfoEnabled}");
                log.Debug($"Input was: {text} - Enabled: {log.IsDebugEnabled}");
                log.Trace($"Input was: {text} - Enabled: {log.IsTraceEnabled}");
            }
            catch (Exception ex)
            {
                log.Error($"Execution failed: {ex.Message}", ex);
            }
        }

        private string PrivateImplementation(string message, bool @throw)
        {
            if (@throw)
                throw new InvalidOperationException("Cannot execute...");

            return $"Passed message was: {message}";
        }
    }
}
