using AddUp.CommonLogging.Simple;
using AddUp.CommonLogging;

namespace TestApp
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(LogLevel.Trace, true, true, true, "yyyy/MM/dd HH:mm:ss:fff", true);
            var log = LogManager.GetLogger(typeof(Program));

            var netfx =
#if NETFRAMEWORK
                ".NET Framework";
#else
                ".NET Core";
#endif
            log.Info($"Message from AddUp.CommonLogging Test App ({netfx})");
            App.Run();
        }
    }
}
