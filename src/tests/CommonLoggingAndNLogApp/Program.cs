using Common.Logging;
using Common.Logging.Configuration;
using Common.Logging.NLog;

namespace TestApp
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
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
            var ok = log.IsInfoEnabled;
            log.Info($"Message from Common.Logging+NLog Test App ({netfx}) - Enabled: {ok}");
            App.Run();
        }
    }
}
