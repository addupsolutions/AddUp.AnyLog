using Common.Logging;
using Common.Logging.Configuration;
using Common.Logging.NLog;

namespace TestApp
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // See https://github.com/net-commons/common-logging/issues/153
            // We use our implementation of ConsoleOutLogger because Common.Logging only provides one for netfx
            var props = new NameValueCollection
            {
                { "configType", "FILE" },
                { "configFile", "./NLog.config" }
            };
            
            LogManager.Adapter = new NLogLoggerFactoryAdapter(props);
            var log = LogManager.GetLogger(typeof(Program));

            var netfx =
#if NETFRAMEWORK
                ".NET Framework";
#else
                ".NET Core";
#endif
            log.Info($"Message from Common.Logging+NLog Test App ({netfx})");
            App.Run();
        }
    }
}
