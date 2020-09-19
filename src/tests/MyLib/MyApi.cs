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
                log.Fatal($"Input was: {text}");
                log.Error($"Input was: {text}");
                log.Warn($"Input was: {text}");
                log.Info($"Input was: {text}");
                log.Debug($"Input was: {text}");
                log.Trace($"Input was: {text}");
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
