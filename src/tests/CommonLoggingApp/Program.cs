using Common.Logging;

namespace TestApp
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // See https://github.com/net-commons/common-logging/issues/153
            // We use our implementation of ConsoleOutLogger because Common.Logging only provides one for netfx
            LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(LogLevel.Debug, true, true, true, "yyyy/MM/dd HH:mm:ss:fff", true);
            var log = LogManager.GetLogger(typeof(Program));

            var netfx =
#if NETFRAMEWORK
                ".NET Framework";
#else
                ".NET Core";
#endif

            var ok = log.IsInfoEnabled;
            log.Info($"Message from Common.Logging Test App ({netfx}) - Enabled: {ok}");
            App.Run();
        }
    }
}
